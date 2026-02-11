using Core.Models;
using Core.Models.Filters;

namespace Core.Interfaces;

public interface IReligiousOrdersRepository
{
    Task<PagedResult<ReligiousOrder>> GetAllAsync(EntityFilters filters);
    Task<ReligiousOrder?> GetByIdAsync(int id);
    Task<bool> CreateAsync(ReligiousOrder religiousOrder);
    Task<bool> UpdateAsync(ReligiousOrder religiousOrder);
    Task DeleteAsync(int id);
}
