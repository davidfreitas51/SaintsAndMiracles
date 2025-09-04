using Core.DTOs;

namespace Core.Interfaces.Services;

public interface IPrayersService
{
    Task<int?> CreatePrayerAsync(NewPrayerDto newPrayer, string userId);
    Task<bool> UpdatePrayerAsync(int id, NewPrayerDto updatedPrayer, string userId);
    Task DeletePrayerAsync(string slug, string userId);
}
