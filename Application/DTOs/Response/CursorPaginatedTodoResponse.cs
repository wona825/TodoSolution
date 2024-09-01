using System.Text.Json.Serialization;

namespace Application.DTOs.Response
{
	public record CursorPaginatedTodoResponse
	(
        [property: JsonPropertyName("todos")] IEnumerable<TodoResponse> Todos,
        [property: JsonPropertyName("total_count")] int TotalCount,
        [property: JsonPropertyName("next_cursor")] string? NextCursor,
        [property: JsonPropertyName("page_size")] int PageSize
    );
}
