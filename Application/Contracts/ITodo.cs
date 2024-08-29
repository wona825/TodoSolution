using Application.DTOs.Request;
using Application.DTOs.Response;
using Domain.Enums;

namespace Application.Contracts
{
	public interface ITodo
	{
        Task<int> ImportTodosAsync(Stream fileStream);
        Task<PagedTodoResponse> GetAllTodosAsync(TodoStatus? todoStatus, string? username, string? search, int? pageNum, int? pageSize);
        Task UpdateTodoStatusAsync(int id, TodoStatus newStatus, int userId);
        Task UpdateTodoDetailsAsync(int id, UpdateTodoDetailsRequest updateTodoDetailsRequest, int userId);
    }
}

