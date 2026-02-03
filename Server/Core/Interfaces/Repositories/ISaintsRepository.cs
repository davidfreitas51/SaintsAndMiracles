using Core.Models;
using Core.Models.Filters;

namespace Core.Interfaces;

public interface ISaintsRepository
{
    Task<PagedResult<Saint>> GetAllAsync(SaintFilters filters);
    Task<Saint?> GetByIdAsync(int id);
    Task<Saint?> GetBySlugAsync(string slug);
    Task<bool> SlugExistsAsync(string slug);
    Task<bool> CreateAsync(Saint saint);
    Task<bool> UpdateAsync(Saint saint);
    Task DeleteAsync(int id);
    Task<IReadOnlyList<string>> GetCountriesAsync();
    Task<List<Saint>> GetSaintsOfTheDayAsync(DateOnly feastDay);
    Task<List<Saint>> GetUpcomingFeasts(DateOnly today, int take = 10);
    Task<int> GetTotalSaintsAsync();
}
