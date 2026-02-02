namespace Core.Models;

public class MiracleFilters
{
    public MiracleOrderBy OrderBy { get; set; } = MiracleOrderBy.Title;
    public string Country { get; set; } = "";
    public string Century { get; set; } = "";
    public string Search { get; set; } = "";
    public List<int>? TagIds { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
