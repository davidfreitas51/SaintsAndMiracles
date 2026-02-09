using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

public class NewSaintDto
{
    [Required]
    [PersonName]
    [NotOnlyNumbers]
    public required string Name { get; set; }

    [Required]
    [SafeText]
    public required string Country { get; set; }

    [Range(-50, 21)]
    public int Century { get; set; }

    [Required]
    [ImageSource]
    public required string Image { get; set; }

    [Required]
    [SafeText]
    public required string Description { get; set; }

    [Required]
    [SafeText]
    public required string MarkdownContent { get; set; }

    [SafeText]
    public string? Title { get; set; }

    public DateOnly? FeastDay { get; set; }

    [SafeText]
    public string? PatronOf { get; set; }

    public int? ReligiousOrderId { get; set; }

    [MaxItems(5)]
    public List<int>? TagIds { get; set; }

    [SafeSlug]
    public string? Slug { get; set; }
}
