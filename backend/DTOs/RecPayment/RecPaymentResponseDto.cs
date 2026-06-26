using System.Text.Json.Serialization;

namespace backend.DTOs.RecPayment;

public class RecPaymentResponseDto
{
    [JsonPropertyName("id_uzytkownika")]
    public int UserId { get; set; }

    [JsonPropertyName("id_kategorii")]
    public int? CategoryId { get; set; }

    [JsonPropertyName("id_t_powtarzalnej")]
    public int RecPaymentId { get; set; }

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

    [JsonPropertyName("co_ile")]
    public string Interval { get; set; } = string.Empty;

    [JsonPropertyName("nastepny_termin")]
    public DateOnly NextDate { get; set; }

    [JsonPropertyName("czy_aktywna")]
    public bool IsActive { get; set; }
}