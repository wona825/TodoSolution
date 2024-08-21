using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Application.DTOs.Request
{
    public class RegisterUserRequest
    {
        [Required]
        [property: JsonPropertyName("username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [property: JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}
