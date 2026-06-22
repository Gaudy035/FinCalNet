using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_uzytkownik")]
public class User
{
    [Key]
    [Column("id_uzytkownika")]
    public int UserId { get; set; }

    [Required]
    [MaxLength(30)]
    [Column("imie")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    [Column("nazwisko")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    
    [Required]
    [MaxLength(255)]
    [Column("haslo")]
    public string Password { get; set; } = string.Empty;

    [Column("data_zalozenia")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("czy_aktywny")]
    public bool IsActive { get; set; } = true;

    [Column("data_usuniecia")]
    public DateTime? DeletedAt { get; set; }

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<RecPayment> RecPayments { get; set; } = new List<RecPayment>();
}