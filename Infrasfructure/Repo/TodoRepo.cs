using System.Globalization;
using System.Net;
using Application.Contracts;
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
                        DisabledAt = disabledAt
                    };
                    todos.Add(todo);
                }
            }
            _appDbContext.Todos.AddRange(todos);
            await _appDbContext.SaveChangesAsync();

            return todos.Count;
        }


        public async Task<PagedTodoResponse> GetAllTodosAsync(TodoStatus? todoStatus, int? pageNum, int? pageSize)
        {
            var query = _appDbContext.Todos
                .Include(todo => todo.Owner)
                .Where(todo => todo.DisabledAt == null);

            if (todoStatus.HasValue)
            {
                query = query.Where(todo => todo.Status == todoStatus.Value);
            }

            int totalCount = await query.CountAsync();

            // 기본 페이지 크기 설정
            int defaultPageSize = pageSize ?? totalCount;

            int defaultPageNum;

            if (totalCount != 0)
            {
                defaultPageNum = pageNum ?? 1; 
            }
            else
            {
                defaultPageNum = 0; 
            }

            // 페이지 계산
            int skip = (defaultPageNum - 1) * defaultPageSize;

            if (skip > totalCount || (totalCount > 0 && defaultPageNum == 0))
            {
                throw new CustomException(HttpStatusCode.NotFound, "Requested page does not exist."); 
            }

            var todos = await query.Skip(skip).Take(defaultPageSize).ToListAsync();

            // 전체 페이지 수 계산
            int totalPages = (int)Math.Ceiling(totalCount / (double)defaultPageSize);

            return new PagedTodoResponse(
                todos.Select(todo => new TodoResponse(
                    todo.Id,
                    todo.OwnerId,
                    todo.Owner?.UserName ?? string.Empty,
                    todo.Title,
                    todo.Description,
                    todo.Status.ToString(),
                    todo.CreatedAt
                )),
                totalCount,
                totalPages,
                defaultPageNum
            );
        }
    }
}
  