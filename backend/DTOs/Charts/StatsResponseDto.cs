using System.Text.Json.Serialization;

namespace backend.DTOs.Charts;

public class StatsResponseDto
{
    [JsonPropertyName("kategoria")]
    public string CategoryName { get; set; } = string.Empty;

    [JsonPropertyName("kwota")]
    public double Amount { get; set; }
}