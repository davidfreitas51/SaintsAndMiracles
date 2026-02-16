using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ReligiousOrdersRepository(DataContext context, ICacheService cacheService, ILogger<ReligiousOrdersRepository> logger) : IReligiousOrdersRepository
{
    public async Task<PagedResult<ReligiousOrder>> GetAllAsync(EntityFilters filters)
    {
        var cacheKey = cacheService.BuildKey(
            "religious_order",
            $"list_page{filters.Page}_size{filters.PageSize}_search{filters.Search}",
            incrementVersion: false
        );

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var query = context.ReligiousOrders.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filters.Search))
                {
                    query = query.Where(ro => ro.Name.Contains(filters.Search));
                }

                var total = await query.CountAsync();

                var items = await query
                    .OrderBy(ro => ro.Name)
                    .Skip((filters.Page - 1) * filters.PageSize)
                    .Take(filters.PageSize)
                    .ToListAsync();

                return new PagedResult<ReligiousOrder>
                {
                    Items = items,
                    TotalCount = total
                };
            }
        );

        return result ?? new PagedResult<ReligiousOrder>();
    }

    public async Task<ReligiousOrder?> GetByIdAsync(int id)
    {
        var cacheKey = cacheService.BuildKey("religious_order", $"id{id}", incrementVersion: false);

        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => context.ReligiousOrders.FindAsync(id).AsTask()
        );
    }

    public async Task<bool> CreateAsync(ReligiousOrder order)
    {
        context.ReligiousOrders.Add(order);
        var created = await context.SaveChangesAsync() > 0;

        if (created)
        {
            InvalidateCaches(order);
            logger.LogInformation("Religious order created in database. Id={Id}, Name={Name}", order.Id, order.Name);
        }
        else
        {
            logger.LogWarning("Religious order creation failed to save. Name={Name}", order.Name);
        }

        return created;
    }

    public async Task<bool> UpdateAsync(ReligiousOrder order)
    {
        context.ReligiousOrders.Update(order);
        var updated = await context.SaveChangesAsync() > 0;

        if (updated)
        {
            InvalidateCaches(order);
            logger.LogInformation("Religious order updated in database. Id={Id}, Name={Name}", order.Id, order.Name);
        }
        else
        {
            logger.LogWarning("Religious order update failed to save. Id={Id}, Name={Name}", order.Id, order.Name);
        }

        return updated;
    }

    public async Task DeleteAsync(int id)
    {
        var order = await context.ReligiousOrders.FindAsync(id);
        if (order is null)
        {
            logger.LogWarning("Delete failed: Religious order not found. Id={Id}", id);
            return;
        }

        context.ReligiousOrders.Remove(order);
        var deleted = await context.SaveChangesAsync() > 0;

        if (deleted)
        {
            InvalidateCaches(order);
            logger.LogInformation("Religious order deleted from database. Id={Id}, Name={Name}", id, order.Name);
        }
        else
        {
            logger.LogWarning("Religious order deletion failed to save. Id={Id}", id);
        }
    }

    private void InvalidateCaches(ReligiousOrder order)
    {
        cacheService.GetNextVersion("religious_order");

        logger.LogInformation("Religious order caches invalidated. Id={Id}, Name={Name}", order.Id, order.Name);
    }
}
