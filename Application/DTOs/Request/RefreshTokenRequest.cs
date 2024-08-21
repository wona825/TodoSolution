using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTOs.Request
{
    public class RefreshTokenRequest
    {
        [Required]
        [property: JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        [property: JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
