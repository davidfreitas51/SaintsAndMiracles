using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

public class NewSaintDto
{
    [Required]
    [PersonName]
    [NotOnlyNumbers]
    [StringLength(150, MinimumLength = 1)]
    public required string Name { get; set; }

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
    [StringLength(100, MinimumLength = 1)]
    public string? Title { get; set; }

    public DateOnly? FeastDay { get; set; }

    [SafeText]
    [StringLength(100, MinimumLength = 1)]
    public string? PatronOf { get; set; }

    public int? ReligiousOrderId { get; set; }

    [MaxItems(5)]
    public List<int>? TagIds { get; set; }

    [SafeSlug]
    public string? Slug { get; set; }
}