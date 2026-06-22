using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace backend.DTOs.User;

public class LoginResponseDto
{
    [JsonPropertyName("access_token")]
    [Required]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "bearer";

    [JsonPropertyName("message")]
    public string Message { get; set; } = "Zalogowano pomyslnie";
}