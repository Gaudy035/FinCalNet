using System.Text.Json.Serialization;

namespace backend.DTOs.User;

public class RegisterDto
{
    [JsonPropertyName("imie")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonPropertyName("nazwisko")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("haslo")]
    public string Password { get; set; } = string.Empty;


}