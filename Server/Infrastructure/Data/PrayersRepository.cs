using Core.Interfaces;
using Core.Models;
using Core.Models.Filters;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class PrayersRepository(DataContext context) : IPrayersRepository
{
    public async Task<PagedResult<Prayer>> GetAllAsync(PrayerFilters filters)
    {
        var query = context.Prayers
            .Include(p => p.Tags)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.ToLower();
            query = query.Where(p =>
                EF.Functions.Like(p.Title.ToLower(), $"%{search}%") ||
                EF.Functions.Like(p.Description.ToLower(), $"%{search}%"));
        }

        if (filters.TagIds is { Count: > 0 })
        {
            query = query.Where(p => p.Tags.Any(tag => filters.TagIds.Contains(tag.Id)));
        }

        query = string.IsNullOrWhiteSpace(filters.OrderBy)
            ? query.OrderBy(p => p.Title)
            : filters.OrderBy.ToLower() switch
            {
                "title" => query.OrderBy(p => p.Title),
                "title_desc" => query.OrderByDescending(p => p.Title),
                _ => query.OrderBy(p => p.Title)
            };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PagedResult<Prayer>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }

    public async Task<Prayer?> GetByIdAsync(int id)
    {
        return await context.Prayers
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Prayer?> GetBySlugAsync(string slug)
    {
        return await context.Prayers
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Slug == slug);
    }

    public async Task<bool> CreateAsync(Prayer newPrayer)
    {
        newPrayer.CreatedAt = DateTime.UtcNow;
        newPrayer.UpdatedAt = DateTime.UtcNow;
        await context.Prayers.AddAsync(newPrayer);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Prayer prayer)
    {
        prayer.UpdatedAt = DateTime.UtcNow;
        context.Prayers.Update(prayer);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Prayer prayer)
    {
        context.Prayers.Remove(prayer);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        return await context.Prayers.AnyAsync(p => p.Slug == slug);
    }

    public async Task<IReadOnlyList<string>> GetTagsAsync()
    {
        return await context.Prayers
            .SelectMany(p => p.Tags)
            .Select(t => t.Name)
            .Distinct()
            .ToListAsync();
    }

    public async Task<int> GetTotalPrayersAsync()
    {
        return await context.Prayers.CountAsync();
    }
}
