using System.Net;
using System.Security.Claims;
using Application.Contracts;
using Application.DTOs.Request;
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
        public async Task<ActionResult<PagedTodoResponse>> GetAllTodosAsync(
            [ModelBinder(BinderType = typeof(EnumModelBinder<TodoStatus>), Name = "todo_status")] TodoStatus? todoStatus,
            [FromQuery] string? username,
            [FromQuery] string? title,
            [FromQuery(Name = "page_number")] int? pageNumber,
            [FromQuery(Name = "page_size")] int? pageSize)
        {
            var todos = await _todo.GetAllTodosAsync(todoStatus, username, title, pageNumber, pageSize);
            return Ok(todos);
        }

        [HttpPatch("{id}/status"), Authorize]
        public async Task<IActionResult> UpdateTodoStatusAsync(
            int id,
            [ModelBinder(BinderType = typeof(EnumModelBinder<TodoStatus>), Name = "new_status")] TodoStatus newStatus)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new CustomException(HttpStatusCode.Unauthorized, "User ID not found in token."));

            await _todo.UpdateTodoStatusAsync(id, newStatus, userId);
            return Ok();
        }

        [HttpPatch("{id}"), Authorize]
        public async Task<IActionResult> UpdateTodoDetailsAsync(
            int id,
            [FromBody] UpdateTodoDetailsRequest updateTodoDetailsRequest)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new CustomException(HttpStatusCode.Unauthorized, "User ID not found in token."));

            await _todo.UpdateTodoDetailsAsync(id, updateTodoDetailsRequest, userId);
            return Ok();
        }

        [HttpDelete("{id}"), Authorize]
        public async Task<IActionResult> SoftDeleteTodoAsync(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new CustomException(HttpStatusCode.Unauthorized, "User ID not found in token."));

            await _todo.SoftDeleteTodoAsync(id, userId);
            return Ok();
        }
    }
}
