using System.ComponentModel.DataAnnotations.Schema;

namespace HizliOgren.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    public int CourseId { get; set; }
    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }
}
