using System.Text.Json.Serialization;

namespace Application.DTOs.Response
{
    public record RegisterUserResponse(
        [property: JsonPropertyName("user_id")] int UserId,
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken
    );
}
