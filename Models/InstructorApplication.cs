using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

/// <summary>Eğitmen olmak isteyen kullanıcıların başvuru formu kaydı.</summary>
public class InstructorApplication
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string FullName { get; set; } = "";

    [Required, MaxLength(256)]
    public string Email { get; set; } = "";

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>Deneyim, neden eğitmen olmak istediği vb.</summary>
    [MaxLength(2000)]
    public string? Message { get; set; }

    /// <summary>Giriş yapmış kullanıcı ise UserId dolu olur.</summary>
    public int? UserId { get; set; }
    public User? User { get; set; }

    /// <summary>Beklemede, Onaylandı, Reddedildi</summary>
    public InstructorApplicationStatus Status { get; set; } = InstructorApplicationStatus.Pending;

    [MaxLength(2000)]
    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum InstructorApplicationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
