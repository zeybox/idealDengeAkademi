using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

/// <summary>Admin: Yeni kullanıcı ekleme formu için model.</summary>
public class AdminUserCreateModel
{
    [Required(ErrorMessage = "Ad soyad gereklidir.")]
    [MaxLength(200)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "E-posta gereklidir.")]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre gereklidir.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [MaxLength(256)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

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
}
