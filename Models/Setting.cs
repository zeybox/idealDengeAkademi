using System.ComponentModel.DataAnnotations;

namespace HizliOgren.Models;

public class Setting
{
    [Key, MaxLength(100)]
    public string Key { get; set; } = "";

    public string? Value { get; set; }
}
