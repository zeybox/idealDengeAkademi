using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [Required, MaxLength(100)]
    public string Slug { get; set; } = "";

    public int SortOrder { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
