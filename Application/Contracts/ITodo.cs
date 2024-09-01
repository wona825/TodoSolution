using Application.DTOs.Request;
using Application.DTOs.Response;
using Domain.Enums;

namespace Application.Contracts
{
	public interface ITodo
	{
        Task<int> ImportTodosAsync(Stream fileStream);
        Task<OffsetPaginatedTodoResponse> GetAllTodosWithOffsetPaginationAsync(TodoStatus? todoStatus, string? username, string? title, int? pageNum, int? pageSize);
        Task<CursorPaginatedTodoResponse> GetAllTodosWithCustomCursorPaginationAsync(TodoStatus? todoStatus, string? username, string? title, string? cursor, int? size);
        Task UpdateTodoStatusAsync(int id, TodoStatus newStatus, int userId);
        Task UpdateTodoDetailsAsync(int id, UpdateTodoDetailsRequest updateTodoDetailsRequest, int userId);
        Task SoftDeleteTodoAsync(int id, int userId);
    }
}

