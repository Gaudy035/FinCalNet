using System.Text.Json.Serialization;

namespace backend.DTOs.User;

public class LoginDto
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("haslo")]
    public string Password { get; set; } = string.Empty;
}