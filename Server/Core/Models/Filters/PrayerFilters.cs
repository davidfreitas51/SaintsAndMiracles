using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.Models.Filters;

public class PrayerFilters
{
    public PrayerOrderBy OrderBy { get; set; } = PrayerOrderBy.Title;

    [SafeText]
    [MaxLength(100, ErrorMessage = "Search must be at most 100 characters.")]
    public string Search { get; set; } = "";

    [MaxItems(10, ErrorMessage = "TagIds can contain at most 10 items.")]
    public List<int>? TagIds { get; set; }

    [Range(1, 1000, ErrorMessage = "PageNumber must be between 1 and 1000.")]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 25;
}
