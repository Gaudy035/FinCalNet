using System.Text.Json.Serialization;

namespace backend.DTOs.Charts;

public class SummaryResponseDto
{
    [JsonPropertyName("typ")]
    public string TypeName { get; set; } = string.Empty;

    [JsonPropertyName("kwota")]
    public double Amount { get; set; }
}