using System.Text.Json.Serialization;

namespace backend.DTOs.Payment;

public class PaymentCreateDto
{
    [JsonPropertyName("id_uzytkownika")]
    public int UserId { get; set; }

    [JsonPropertyName("id_kategorii")]
    public int CategoryId { get; set; }

    [JsonPropertyName("typ")]
    public string PaymentType { get; set; } = string.Empty;

    [JsonPropertyName("tytul")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("opis")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("kwota")]
    public double Amount { get; set; }

    [JsonPropertyName("metoda")]
    public string PaymentMethod { get; set; } = string.Empty;

    [JsonPropertyName("konto")]
    public string? Account { get; set; }
    
    [JsonPropertyName("wlasciciel_konta")]
    public string? AccountOwner { get; set; }

    [JsonPropertyName("data")]
    public DateOnly Date { get; set; }
}