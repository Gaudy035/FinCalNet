using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_t_powtarzalne")]
public class RecPayment
{
    [Key]
    [Column("id_t_powtarzalnej")]
    public int RecPaymentId { get; set; }

    [Column("id_uzytkownika")]
    public int UserId { get; set; }

    [Column("id_kategorii")]
    public int? CategoryId { get; set; }

    [Required]
    [MaxLength(10)]
    [Column("typ")]
    public string PaymentType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    [Column("tytul")]
    public string Title { get; set; } = string.Empty;

    [Column("opis")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    [Column("metoda")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required]
    [Column("kwota")]
    public double Amount { get; set; }

    [MaxLength(50)]
    [Column("konto")]
    public string? Account { get; set; }

    [MaxLength(100)]
    [Column("wlasciciel_konta")]
    public string? AccountOwner { get; set; }

    [MaxLength(10)]
    [Column("co_ile")]
    public string Interval { get; set; } = string.Empty;

    [Column("nastepny_termin")]
    public DateOnly NextDate { get; set; }

    [Column("czy_aktywna")]
    public bool IsActive { get; set; } = true;

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;

    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }
}