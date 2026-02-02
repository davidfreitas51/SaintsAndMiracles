public class SaintFilters
{
    public SaintOrderBy OrderBy { get; set; } = SaintOrderBy.Name;
    public string Country { get; set; } = "";
    public string Century { get; set; } = "";
    public string Search { get; set; } = "";
    public string FeastMonth { get; set; } = "";
    public string ReligiousOrderId { get; set; } = "";
    public List<int> TagIds { get; set; } = [];
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
