using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HizliOgren.Models;

public class BlogPost
{
    public int Id { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = "";

    [Required, MaxLength(200)]
    public string Slug { get; set; } = "";

    public string? Excerpt { get; set; }

    public string? Content { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int? AuthorId { get; set; }
    [ForeignKey(nameof(AuthorId))]
    public User? Author { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
