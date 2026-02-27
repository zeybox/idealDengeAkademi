using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HizliOgren.Models;

public class CurriculumItem
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    [ForeignKey(nameof(CourseId))]
    public Course Course { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Type { get; set; } = ""; // video, pdf, article

    [Required, MaxLength(300)]
    public string Title { get; set; } = "";

    [MaxLength(500)]
    public string? YoutubeUrl { get; set; }

    [MaxLength(300)]
    public string? Extra { get; set; }

    /// <summary>Video süresi (dakika). Sadece Type=video için kullanılır.</summary>
    public int? DurationMinutes { get; set; }

    public int SortOrder { get; set; }
}
