using System.Text.Json.Serialization;

namespace Application.DTOs.Response
{
    public record RefreshTokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken
    );
}
