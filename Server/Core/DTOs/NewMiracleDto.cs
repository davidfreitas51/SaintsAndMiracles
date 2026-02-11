using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

public class NewMiracleDto
{
    [Required]
    [SafeText]
    [NotOnlyNumbers]
    [StringLength(150, MinimumLength = 3)]
    public required string Title { get; set; }

    [Required]
    [SafeText]
    [StringLength(150, MinimumLength = 3)]
    public required string Country { get; set; }

    [Required]
    [Range(-20, 21)]
    public int Century { get; set; }

    [Required]
    [ImageSource]
    public required string Image { get; set; }

    [Required]
    [SafeText]
    [StringLength(200, MinimumLength = 1)]
    public required string Description { get; set; }

    [Required]
    [SafeText]
    [StringLength(20000, MinimumLength = 1)]
    public required string MarkdownContent { get; set; }

    [SafeText]
    [StringLength(50, MinimumLength = 1)]
    public string? Date { get; set; }

    [SafeText]
    [StringLength(150, MinimumLength = 1)]
    public string? LocationDetails { get; set; }

    [MaxItems(5)]
    public List<int>? TagIds { get; set; }
}