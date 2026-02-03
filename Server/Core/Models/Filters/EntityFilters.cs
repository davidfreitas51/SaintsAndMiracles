using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.Models.Filters;

public class EntityFilters
{
    [SafeText]
    [MaxLength(50, ErrorMessage = "TagType must be at most 50 characters.")]
    public string TagType { get; set; } = "";

    [SafeText]
    [MaxLength(100, ErrorMessage = "Search must be at most 100 characters.")]
    public string? Search { get; set; }

    [Range(1, 1000, ErrorMessage = "Page must be between 1 and 1000.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 20;

    public int? Type { get; set; }
}
