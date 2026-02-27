using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(256)]
    public string Email { get; set; } = "";

    [MaxLength(256)]
    public string PasswordHash { get; set; } = "";

    [MaxLength(200)]
    public string FullName { get; set; } = "";

    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>Eğitmen unvanı (örn. Yazılım Eğitmeni).</summary>
    [MaxLength(150)]
    public string? Title { get; set; }

    /// <summary>Kısa biyografi / hakkında.</summary>
    [MaxLength(2000)]
    public string? Bio { get; set; }

    /// <summary>Profil fotoğrafı URL.</summary>
    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    public UserRole Role { get; set; }

    [MaxLength(500)]
    public string? GoogleId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Course> CoursesAsInstructor { get; set; } = new List<Course>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<UserCourse> PurchasedCourses { get; set; } = new List<UserCourse>();
}
