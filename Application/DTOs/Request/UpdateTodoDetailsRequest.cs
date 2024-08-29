using System.Text.Json.Serialization;

namespace Application.DTOs.Request
{
	public class UpdateTodoDetailsRequest
	{
        [property: JsonPropertyName("title")]
        public string? Title { get; set; }

        [property: JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}

