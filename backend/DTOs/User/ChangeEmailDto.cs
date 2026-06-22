using System.Text.Json.Serialization;

namespace backend.DTOs.User;

public class ChangeEmailDto
{
    [JsonPropertyName("current_pass")]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [JsonPropertyName("new_email")]
    public string NewEmail { get; set; } = string.Empty;
}