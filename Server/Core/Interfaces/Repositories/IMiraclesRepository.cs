using Core.Models;

namespace Core.Interfaces;

public interface IMiraclesRepository
{
    Task<PagedResult<Miracle>> GetAllAsync(MiracleFilters filters);
    Task<Miracle?> GetByIdAsync(int id);
    Task<Miracle?> GetBySlugAsync(string slug);
    Task<bool> SlugExistsAsync(string slug);
    Task<bool> CreateAsync(Miracle miracle);
    Task<bool> UpdateAsync(Miracle miracle);
    Task DeleteAsync(int id);
    Task<IReadOnlyList<string>> GetCountriesAsync();
    Task<int> GetTotalMiraclesAsync();
}
