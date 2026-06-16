using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data.Entities;

[Table("t_kategorie")]
public class Category
{
    [Key]
    [Column("id_kategorii")]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(50)]
    public string nazwa { get; set; } = string.Empty;

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public ICollection<RecPayment> RecPayments { get; set; } = new List<RecPayment>();
}