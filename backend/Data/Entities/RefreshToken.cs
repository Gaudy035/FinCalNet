using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_refresh_token")]
public class RefreshToken
{
    [Key]
    [Column("id_token")]
    public int TokenId { get; set; }

    [Column("id_uzytkownika")]
    [Required]
    public int UserId { get; set; }

    [Column("token_string")]
    [MaxLength(128)]
    [Required]
    public string TokenString { get; set; } = string.Empty;

    [Column("czy_aktywny")]
    public bool IsActive { get; set; }

    [Column("data_wygasniecia")]
    [Required]
    public DateTimeOffset ExpiresAt { get; set; }

    [Column("data_uniewaznienia")]
    public DateTimeOffset RevokedAt { get; set; }

    [ForeignKey("UserId")]
    public User User { get; set; } = null!;
}