using System.Net;
using Application.Contracts;
using Application.DTOs.Response;
using Domain.Enums;
using Infrasfructure.Error;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController(ITodo todo) : ControllerBase
    {
        private readonly ITodo _todo = todo;

        [HttpPost("import"), Authorize]
        public async Task<IActionResult> ImportTodosAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new CustomException(HttpStatusCode.BadRequest, "No file uploaded.");
            }

            using (var stream = file.OpenReadStream())
            {
                int createdCount = await _todo.ImportTodosAsync(stream);
                return Ok(new { Count = createdCount });
            }
        }

        [HttpGet()]
        public async Task<PagedTodoResponse> GetAllTodosAsync(
            [ModelBinder(BinderType = typeof(EnumModelBinder<TodoStatus>), Name = "todo_status")] TodoStatus? todoStatus,
            [FromQuery(Name = "page_number")] int? pageNumber,
            [FromQuery(Name = "page_size")] int? pageSize)
        {
            var todos = await _todo.GetAllTodosAsync(todoStatus, pageNumber, pageSize);
            return todos;
        }
    }
}
