using System.Text.Json.Serialization;

namespace backend.DTOs.User;

public class RegisterResponseDto
{
    [JsonPropertyName("imie")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonPropertyName("nazwisko")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("id_uzytkownika")]
    public int UserId { get; set; }

    [JsonPropertyName("czy_aktywny")]
    public bool IsActive { get; set; } = true;
}