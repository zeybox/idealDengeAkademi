using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HizliOgren.Models;

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = ""; // Pending, Paid, Failed, Refunded

    [MaxLength(100)]
    public string? PayTRPaymentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
