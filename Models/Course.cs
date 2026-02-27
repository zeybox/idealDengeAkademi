using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HizliOgren.Models;

public class Course
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = "";

    public string? Description { get; set; }

    /// <summary>Bu eğitimde neler var? Her satır bir madde (detay sayfasında liste olarak gösterilir).</summary>
    [MaxLength(2000)]
    public string? Highlights { get; set; }

    /// <summary>İçerik özeti açıklaması (detay sayfasında "İçerik Özeti" başlığının altındaki kısa metin).</summary>
    [MaxLength(1000)]
    public string? ContentSummary { get; set; }

    /// <summary>Ücretli kısım: Sadece satın alanların görebileceği zengin içerik (HTML).</summary>
    public string? PaidContent { get; set; }

    public int CategoryId { get; set; }
    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; } = null!;

    public int InstructorId { get; set; }
    [ForeignKey(nameof(InstructorId))]
    public User Instructor { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CurriculumItem> CurriculumItems { get; set; } = new List<CurriculumItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<UserCourse> UserCourses { get; set; } = new List<UserCourse>();
}
