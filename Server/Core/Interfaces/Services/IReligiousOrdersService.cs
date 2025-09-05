using Core.DTOs;
using Core.Models;

namespace Core.Interfaces.Services;

public interface IReligiousOrdersService
{
    Task<ReligiousOrder?> CreateReligiousOrderAsync(NewReligiousOrderDto dto, string userId);
    Task<bool> UpdateReligiousOrderAsync(int id, NewReligiousOrderDto dto, string userId);
    Task<bool> DeleteReligiousOrderAsync(int id, string userId);
}
