using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

public class InstructorApplicationViewModel
{
    [Required(ErrorMessage = "Ad soyad gereklidir.")]
    [Display(Name = "Ad Soyad")]
    [MaxLength(200)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "E-posta gereklidir.")]
    [EmailAddress]
    [Display(Name = "E-posta")]
    [MaxLength(256)]
    public string Email { get; set; } = "";

    [Display(Name = "Telefon")]
    [MaxLength(50)]
    public string? Phone { get; set; }

    [Display(Name = "Şehir")]
    [MaxLength(100)]
    public string? City { get; set; }

    [Display(Name = "Deneyim / Neden eğitmen olmak istiyorsunuz?")]
    [MaxLength(2000)]
    public string? Message { get; set; }

    [Required(ErrorMessage = "Güvenlik kodu gerekli")]
    [Display(Name = "Güvenlik kodu")]
    [StringLength(10)]
    public string? CaptchaCode { get; set; }
}
