using Core.Models;

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
    Task<Saint?> GetSaintOfTheDayAsync(DateOnly feastDay);
    Task<int> GetTotalSaintsAsync();
}
