using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;

namespace Core.Models.Filters;

public class MiracleFilters
{
    public MiracleOrderBy OrderBy { get; set; } = MiracleOrderBy.Title;

    [SafeText]
    [MaxLength(50)]
    public string Country { get; set; } = "";

    [RegularExpression(@"^\d{0,2}$", ErrorMessage = "Century must be up to 2 digits.")]
    public string Century { get; set; } = "";

    [SafeText]
    [MaxLength(100)]
    public string Search { get; set; } = "";

    [MaxItems(10)]
    public List<int>? TagIds { get; set; }

    [Range(1, 1000)]
    public int PageNumber { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 25;
}
