using Core.Models;
using Core.Models.Filters;

namespace Core.Interfaces;

public interface IPrayersRepository
{
    Task<PagedResult<Prayer>> GetAllAsync(PrayerFilters filters);
    Task<Prayer?> GetByIdAsync(int id);
    Task<Prayer?> GetBySlugAsync(string slug);
    Task<bool> CreateAsync(Prayer newPrayer);
    Task<bool> UpdateAsync(Prayer prayer);
    Task<bool> DeleteAsync(Prayer prayer);
    Task<bool> SlugExistsAsync(string slug);
    Task<IReadOnlyList<string>> GetTagsAsync();
    Task<int> GetTotalPrayersAsync();
}
