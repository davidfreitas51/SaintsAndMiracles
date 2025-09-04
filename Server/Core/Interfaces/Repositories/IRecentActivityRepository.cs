using Core.Models;

namespace Core.Interfaces.Repositories;

public interface IRecentActivityRepository
{
    Task<List<RecentActivity>> GetRecentActivitiesAsync(int limit = 5);
    Task LogActivityAsync(string entityName, int entityId, string displayName, string action, string? userId = null);
}
