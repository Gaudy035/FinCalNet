using System.Text.Json.Serialization;

namespace backend.DTOs.Category;

public class CategoryDto
{
    [JsonPropertyName("id_kategorii")]
    public int CategoryId { get; set; }
    
    [JsonPropertyName("nazwa")]
    public string CategoryName { get; set; } = string.Empty;
}