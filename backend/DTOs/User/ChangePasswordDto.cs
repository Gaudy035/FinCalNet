using System.Text.Json.Serialization;

namespace backend.DTOs.User;

public class ChangePasswordDto
{
    [JsonPropertyName("current_pass")]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [JsonPropertyName("new_pass")]
    public string NewPassword { get; set; } = string.Empty;
}