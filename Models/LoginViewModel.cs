using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta gerekli")]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Şifre gerekli")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = "";
}
