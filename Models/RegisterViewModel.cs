using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "E-posta gerekli")]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre gerekli")]
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "Şifre tekrar")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
    public string ConfirmPassword { get; set; } = "";

    [Required(ErrorMessage = "Ad Soyad gerekli")]
    [StringLength(200)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = "";

    [StringLength(100)]
    [Display(Name = "Şehir")]
    public string? City { get; set; }

    [Required(ErrorMessage = "Güvenlik kodu gerekli")]
    [Display(Name = "Güvenlik kodu")]
    [StringLength(10)]
    public string? CaptchaCode { get; set; }
}
