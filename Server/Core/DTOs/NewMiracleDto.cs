using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.DTOs;

public class NewMiracleDto
{
    [Required]
    [SafeText]
    [StringLength(150, MinimumLength = 3)]
    public required string Title { get; set; }

    [Required]
    [SafeText]
    [StringLength(100)]
    public required string Country { get; set; }

    [Range(-20, 21, ErrorMessage = "Century must be between 20 BC and 21 AD.")]
    public int Century { get; set; }

    [Required]
    [ImageSource]
    public required string Image { get; set; }

    [Required]
    [SafeText]
    [StringLength(500)]
    public required string Description { get; set; }

    [Required]
    [SafeText]
    [StringLength(20000)]
    public required string MarkdownContent { get; set; }

    [SafeText]
    [StringLength(50)]
    public string? Date { get; set; }

    [SafeText]
    [StringLength(150)]
    public string? LocationDetails { get; set; }

    [MaxItems(5)]
    public List<int>? TagIds { get; set; }
}