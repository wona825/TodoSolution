using System.Net;
using Application.Contracts;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Domain.Entites;
using Domain.Enums;
using Infrasfructure.Data;
using Infrasfructure.Error;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Infrasfructure.Repo
{
    internal class TodoRepo : ITodo
    {
        private readonly AppDbContext _appDbContext;

        public TodoRepo(AppDbContext appDbContext)
        {
            this._appDbContext = appDbContext;
        }


        /// <summary>
        /// Excel을 통한 투두 데이터 import 메소드 
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        /// <exception cref="CustomException"></exception>
        public async Task<int> ImportTodosAsync(Stream fileStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var todos = new List<Todo>();

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var title = worksheet.Cells[row, 1].Value.ToString();
                    var description = worksheet.Cells[row, 2].Value.ToString();
                    var createdAtString = worksheet.Cells[row, 3].Value.ToString();
                    var disabledAtString = worksheet.Cells[row, 4].Value?.ToString();

                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(createdAtString))
                    {
                        continue;
                    }

                    DateTime createdAt;
                    DateTime? disabledAt = null;

                    // Excel의 날짜 시스템에서 날짜로 변환
                    if (double.TryParse(createdAtString, out double createdAtOADate))
                    {
                        createdAt = DateTime.FromOADate(createdAtOADate).Date.Add(new TimeSpan(0, 0, 0)); // 시간은 00:00:00으로 설정
                    }
                    else
                    {
                        throw new CustomException(HttpStatusCode.BadRequest, $"Invalid createdAt format in row: {createdAtString}");
                    }

                    // disabledAt 파싱 (값이 있을 경우에만 시도)
                    if (!string.IsNullOrEmpty(disabledAtString))
                    {
                        if (double.TryParse(disabledAtString, out double disabledAtOADate))
                        {
                            disabledAt = DateTime.FromOADate(disabledAtOADate).Date.Add(new TimeSpan(0, 0, 0)); // 시간은 00:00:00으로 설정
                        }
                        else
                        {
                            throw new CustomException(HttpStatusCode.BadRequest, $"Invalid disabledAt format in row");
                        }
                    }

                    var todo = new Todo
                    {
                        Title = title,
                        Description = description,
                        Status = TodoStatus.BackLog,
                        CreatedAt = createdAt,
                        UpdatedAt = createdAt,
                        DisabledAt = disabledAt
                    };
                    todos.Add(todo);
                }
            }
            _appDbContext.Todos.AddRange(todos);
            await _appDbContext.SaveChangesAsync();

            return todos.Count;
        }


        /// <summary>
        /// 필터링을 통한 투두 데이터 리스트 조회 메소드 (offset-based-pagination, by updated datetime & id)
        /// </summary>
        /// <param name="todoStatus"></param>
        /// <param name="username"></param>
        /// <param name="title"></param>
        /// <param name="pageNum"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<OffsetPaginatedTodoResponse> GetAllTodosWithOffsetPaginationAsync(TodoStatus? todoStatus, string? username, string? title, int? pageNum, int? pageSize)
        {
            var query = _appDbContext.Todos
                .Include(todo => todo.Owner)
                .Where(todo => todo.DisabledAt == null);

            if (todoStatus.HasValue)
            {
                query = query.Where(todo => todo.Status == todoStatus.Value);
            }

            // 검색 필터링 (Title)
            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(todo =>
                    todo.Title.Contains(title));
            }

            // 검색 필터링 (Username)
            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(todo =>
                    todo.Owner != null && todo.Owner.UserName.Contains(username));
            }

            int totalCount = await query.CountAsync();
            int defaultPageSize = pageSize.GetValueOrDefault(totalCount); 
            int defaultPageNum = Math.Max(pageNum.GetValueOrDefault(1), 1); 
            int skip = (defaultPageNum - 1) * defaultPageSize;

            // UpdatedAt DESC, Id DESC으로 정렬
            var todos = await query
                .OrderByDescending(todo => todo.UpdatedAt)
                .ThenByDescending(todo => todo.Id)
                .Skip(skip)
                .Take(defaultPageSize)
                .ToListAsync();

            int totalPages = (int)Math.Ceiling(totalCount / (double)defaultPageSize);

            return new OffsetPaginatedTodoResponse(
                todos.Select(todo => new TodoResponse(
                    todo.Id,
                    todo.OwnerId,
                    todo.Owner?.UserName ?? string.Empty,
                    todo.Title,
                    todo.Description,
                    todo.Status.ToString(),
                    todo.CreatedAt,
                    todo.UpdatedAt
                )),
                totalCount,
                totalPages,
                defaultPageNum
            );
        }

        /// <summary>
        /// 필터링을 통한 투두 데이터 리스트 조회 메소드 (cursor-based-pagination, by updated datetime & id)
        /// </summary>
        /// <param name="todoStatus"></param>
        /// <param name="username"></param>
        /// <param name="title"></param>
        /// <param name="cursor"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public async Task<CursorPaginatedTodoResponse> GetAllTodosWithCustomCursorPaginationAsync(
            TodoStatus? todoStatus, string? username, string? title, string? cursor, int? size)
        {
            var query = _appDbContext.Todos
                .Include(todo => todo.Owner)
                .Where(todo => todo.DisabledAt == null);

            if (todoStatus.HasValue)
            {
                query = query.Where(todo => todo.Status == todoStatus.Value);
            }

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(todo => todo.Title.Contains(title));
            }

            if (!string.IsNullOrWhiteSpace(username))
            {
                query = query.Where(todo => todo.Owner != null && todo.Owner.UserName.Contains(username));
            }

            int totalCount = await query.CountAsync();
            int defaultPageSize = size.GetValueOrDefault(totalCount);

            // 커서 파싱
            DateTime? cursorUpdatedAt = null;
            int? cursorId = null;

            if (!string.IsNullOrWhiteSpace(cursor))
            {
                var parts = cursor.Split('_');
                if (parts.Length == 2 && DateTime.TryParse(parts[0], out var parsedDate) && int.TryParse(parts[1], out var parsedId))
                {
                    cursorUpdatedAt = parsedDate;
                    cursorId = parsedId;
                }
            }

            // 커서에 따른 필터링
            if (cursorUpdatedAt.HasValue && cursorId.HasValue)
            {
                query = query.Where(todo => todo.UpdatedAt < cursorUpdatedAt
                    || (todo.UpdatedAt == cursorUpdatedAt && todo.Id < cursorId));
            }

            // UpdatedAt DESC, Id DESC으로 정렬
            var todos = await query
                .OrderByDescending(todo => todo.UpdatedAt)
                .ThenByDescending(todo => todo.Id)
                .Take(defaultPageSize)
                .ToListAsync();

            // 다음 페이지의 커서 생성
            string? nextCursor = null;
            if (todos.Count == defaultPageSize)
            {
                var lastTodo = todos.Last();
                nextCursor = $"{lastTodo.UpdatedAt:O}_{lastTodo.Id}";
            }

            return new CursorPaginatedTodoResponse(
                todos.Select(todo => new TodoResponse(
                    todo.Id,
                    todo.OwnerId,
                    todo.Owner?.UserName ?? string.Empty,
                    todo.Title,
                    todo.Description,
                    todo.Status.ToString(),
                    todo.CreatedAt,
                    todo.UpdatedAt
                )),
                totalCount,
                nextCursor,
                defaultPageSize
            );
        }


        /// <summary>
        /// 투두 status 수정 메소드 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="newStatus"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="CustomException"></exception>
        public async Task UpdateTodoStatusAsync(int id, TodoStatus newStatus, int userId)
        {
            var user = await _appDbContext.Users
                .Where(u => u.Id == userId && u.DisabledAt == null)
                .FirstOrDefaultAsync()
                ?? throw new CustomException(HttpStatusCode.NotFound, "User not found.");

            var todo = await _appDbContext.Todos
                .Include(t => t.Owner)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new CustomException(HttpStatusCode.NotFound, "Todo not found.");

            if (todo.OwnerId.HasValue && todo.OwnerId != userId)
            {
                throw new CustomException(HttpStatusCode.Forbidden, "No permission to update todo.");
            }

            if (todo.Status != TodoStatus.BackLog && newStatus == TodoStatus.BackLog)
            {
                throw new CustomException(HttpStatusCode.BadRequest, "Cannot move todo to BackLog status.");
            }

            if (newStatus != TodoStatus.BackLog)
            {
                todo.Status = newStatus;
                todo.Owner = user;
                todo.OwnerId = userId;
            }

            todo.UpdatedAt = DateTime.Now;
            await _appDbContext.SaveChangesAsync();
        }


        /// <summary>
        /// 투두 details(title, description) 수정 메소드 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateTodoDetailsRequest"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="CustomException"></exception>
        public async Task UpdateTodoDetailsAsync(int id, UpdateTodoDetailsRequest updateTodoDetailsRequest, int userId)
        {
            var user = await _appDbContext.Users
                .Where(u => u.Id == userId && u.DisabledAt == null)
                .FirstOrDefaultAsync()
                ?? throw new CustomException(HttpStatusCode.NotFound, "User not found.");

            var todo = await _appDbContext.Todos
                .Include(t => t.Owner)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new CustomException(HttpStatusCode.NotFound, "Todo not found.");

            if (todo.OwnerId != userId)
            {
                throw new CustomException(HttpStatusCode.Forbidden, "No permission to update todo details.");
            }

            if (updateTodoDetailsRequest.Title != null)
            {
                todo.Title = updateTodoDetailsRequest.Title;
            }

            if (updateTodoDetailsRequest.Description != null)
            {
                todo.Description = updateTodoDetailsRequest.Description;
            }

            todo.UpdatedAt = DateTime.Now;
            await _appDbContext.SaveChangesAsync();
        }


        /// <summary>
        /// 투두 소프트 삭제 메소드 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="CustomException"></exception>
        public async Task SoftDeleteTodoAsync(int id, int userId)
        {
            var user = await _appDbContext.Users
                .Where(u => u.Id == userId && u.DisabledAt == null)
                .FirstOrDefaultAsync()
                ?? throw new CustomException(HttpStatusCode.NotFound, "User not found.");

            var todo = await _appDbContext.Todos
                .Include(t => t.Owner)
                .FirstOrDefaultAsync(t => t.Id == id)
                ?? throw new CustomException(HttpStatusCode.NotFound, "Todo not found.");

            if (todo.OwnerId != null && todo.OwnerId != userId)
            {
                throw new CustomException(HttpStatusCode.Forbidden, "No permission to update todo details.");
            }

            todo.DisabledAt = DateTime.Now;
            await _appDbContext.SaveChangesAsync();
        }
    }
}
  