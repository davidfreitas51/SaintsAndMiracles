using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class ReligiousOrdersRepository(DataContext context, ICacheService cacheService) : IReligiousOrdersRepository
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
            InvalidateCaches();

        return created;
    }

    public async Task<bool> UpdateAsync(ReligiousOrder order)
    {
        context.ReligiousOrders.Update(order);
        var updated = await context.SaveChangesAsync() > 0;

        if (updated)
            InvalidateCaches();

        return updated;
    }

    public async Task DeleteAsync(int id)
    {
        var order = await context.ReligiousOrders.FindAsync(id);
        if (order is null) return;

        context.ReligiousOrders.Remove(order);
        await context.SaveChangesAsync();

        InvalidateCaches();
    }

    private void InvalidateCaches()
    {
        cacheService.GetNextVersion("religious_order");
    }
}
