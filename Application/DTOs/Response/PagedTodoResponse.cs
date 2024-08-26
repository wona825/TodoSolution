using System.Text.Json.Serialization;

namespace Application.DTOs.Response
{
	public record PagedTodoResponse
	(
        [property: JsonPropertyName("todos")] IEnumerable<TodoResponse> Todos,
        [property: JsonPropertyName("total_count")] int TotalCount,
        [property: JsonPropertyName("total_pages")] int TotalPages,
        [property: JsonPropertyName("current_page")] int CurrentPage
    );
}

