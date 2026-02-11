using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

public class NewPrayerDto
{
    [Required]
    [SafeText]
    [NotOnlyNumbers]
    [StringLength(150, MinimumLength = 3)]
    public required string Title { get; set; }

    [Required]
    [SafeText]
    [StringLength(200, MinimumLength = 1)]
    public required string Description { get; set; }

    [Required]
    [ImageSource]
    public required string Image { get; set; }

    [Required]
    [SafeText]
    [StringLength(20000, MinimumLength = 1)]
    public required string MarkdownContent { get; set; }

    [MaxItems(5)]
    public List<int>? TagIds { get; set; }
}