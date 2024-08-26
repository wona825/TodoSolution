using System.Text.Json.Serialization;

namespace Application.DTOs.Response
{
	public record TodoResponse
	(
        [property: JsonPropertyName("todo_id")] int Id,
        [property: JsonPropertyName("owner_id")] int? OwnerId,
        [property: JsonPropertyName("owner_name")] string? OwnerName,
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("description")] string Description,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("created_at")] DateTime CreatedAt
    );
}

