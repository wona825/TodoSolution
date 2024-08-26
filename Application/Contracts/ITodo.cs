using Application.DTOs.Response;
using Domain.Enums;

namespace Application.Contracts
{
	public interface ITodo
	{
        Task<int> ImportTodosAsync(Stream fileStream);
        Task<PagedTodoResponse> GetAllTodosAsync(TodoStatus? todoStatus, int? pageNum, int? pageSize);
    }
}

