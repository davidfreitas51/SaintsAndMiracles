namespace Core.Models.Filters;

public class PrayerFilters
{
    public PrayerOrderBy OrderBy { get; set; } = PrayerOrderBy.Title;
    public string Search { get; set; } = "";
    public List<int>? TagIds { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
