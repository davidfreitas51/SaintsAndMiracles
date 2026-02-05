using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class NewPrayerDto
{
    [Required]
    [SafeText]
    public required string Title { get; set; }

    [Required]
    [SafeText]
    public required string Description { get; set; }

    [Required]
    public string? Image { get; set; }

    [Required]
    [SafeText]
    public required string MarkdownContent { get; set; }

    [MaxItems(5)]
    public List<int>? TagIds { get; set; }
}
