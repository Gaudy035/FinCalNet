using System.Text.Json.Serialization;

namespace backend.DTOs.User;

public class RefreshResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
}