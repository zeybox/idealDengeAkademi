using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

/// <summary>Admin üye düzenleme formu için model. Şifre alanı veritabanındaki PlainPassword ile doldurulur.</summary>
public class AdminUserEditModel
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string FullName { get; set; } = "";

    [Required, MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = "";

    public UserRole Role { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    /// <summary>Veritabanındaki PlainPassword ile doldurulur; admin değiştirebilir.</summary>
    [MaxLength(256)]
    public string? Password { get; set; }
}
