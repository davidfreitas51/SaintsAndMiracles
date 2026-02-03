using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class NewMiracleDto
{
    [Required]
    [SafeText]
    public required string Title { get; set; }

    [Required]
    [SafeText]
    public required string Country { get; set; }

    public int Century { get; set; }

    [Required]
    [SafePath]
    public required string Image { get; set; }

    [Required]
    [SafeText]
    public required string Description { get; set; }

    [Required]
    [SafeText]
    public required string MarkdownContent { get; set; }

    [SafeText]
    public string? Date { get; set; }

    [SafeText]
    public string? LocationDetails { get; set; }

    [MaxItems(5)]
    public List<int>? TagIds { get; set; }
}
