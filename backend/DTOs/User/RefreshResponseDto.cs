using System.Text.Json.Serialization;

namespace backend.DTOs.User;

public class RefreshResponseDto
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = "Odswiezono token";

    [JsonIgnore]
    public string RefreshToken { get; set; } = string.Empty;
}