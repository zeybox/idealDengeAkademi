using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

public class HeroSlide
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = "";

    [MaxLength(1000)]
    public string? Text { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(200)]
    public string? ImageAlt { get; set; }

    public int SortOrder { get; set; }
}
