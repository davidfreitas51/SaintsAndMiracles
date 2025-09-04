using Core.Enums;
using Core.Models;

namespace Core.Interfaces.Repositories;

public interface IRecentActivityRepository
{
    Task<PagedResult<RecentActivity>> GetRecentActivitiesAsync(int pageNumber, int pageSize);
    Task LogActivityAsync(EntityType entityName, int entityId, string displayName, ActivityAction action, string? userId = null);
}
