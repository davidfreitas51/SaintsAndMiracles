namespace Core.Models;

public class EntityFilters
{
    public string TagType { get; set; } = "";
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? Type { get; set; }
}