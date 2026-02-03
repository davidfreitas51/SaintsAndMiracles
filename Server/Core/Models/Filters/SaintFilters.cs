using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.Models.Filters;

public class SaintFilters
{
    public SaintOrderBy OrderBy { get; set; } = SaintOrderBy.Name;

    [SafeText]
    [MaxLength(50, ErrorMessage = "Country must be at most 50 characters.")]
    public string Country { get; set; } = "";

    [SafeText]
    [MaxLength(20, ErrorMessage = "Century must be at most 20 characters.")]
    public string Century { get; set; } = "";

    [SafeText]
    [MaxLength(100, ErrorMessage = "Search must be at most 100 characters.")]
    public string Search { get; set; } = "";

    [SafeText]
    [MaxLength(20, ErrorMessage = "FeastMonth must be at most 20 characters.")]
    public string FeastMonth { get; set; } = "";

    [SafeText]
    [MaxLength(20, ErrorMessage = "ReligiousOrderId must be at most 20 characters.")]
    public string ReligiousOrderId { get; set; } = "";

    [MaxItems(10, ErrorMessage = "TagIds can contain at most 10 items.")]
    public List<int> TagIds { get; set; } = new();

    [Range(1, 1000, ErrorMessage = "PageNumber must be between 1 and 1000.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 25;
}
