using Core.Interfaces;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class SaintsRepository(DataContext context) : ISaintsRepository
{
    public async Task<PagedResult<Saint>> GetAllAsync(SaintFilters filters)
    {
        var query = context.Saints
            .Include(s => s.Tags)
            .Include(s => s.ReligiousOrder)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters.Country))
        {
            query = query.Where(s => s.Country == filters.Country);
        }

        if (!string.IsNullOrWhiteSpace(filters.Century))
        {
            query = query.Where(s => s.Century.ToString() == filters.Century);
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.ToLower();

            query = query.Where(s =>
                EF.Functions.Like(s.Name.ToLower(), $"%{search}%") ||
                EF.Functions.Like(s.Description.ToLower(), $"%{search}%"));
        }

        if (!string.IsNullOrWhiteSpace(filters.FeastMonth))
        {
            if (int.TryParse(filters.FeastMonth, out var month))
            {
                query = query.Where(s => s.FeastDay.HasValue && s.FeastDay.Value.Month == month);
            }
        }

        if (!string.IsNullOrWhiteSpace(filters.ReligiousOrderId))
        {
            if (int.TryParse(filters.ReligiousOrderId, out var orderId))
            {
                query = query.Where(s => s.ReligiousOrderId == orderId);
            }
        }

        if (filters.TagIds is { Count: > 0 })
        {
            query = query.Where(s => s.Tags.Any(tag => filters.TagIds.Contains(tag.Id)));
        }

        query = string.IsNullOrWhiteSpace(filters.OrderBy)
            ? query.OrderBy(s => s.Name)
            : filters.OrderBy.ToLower() switch
            {
                "name" => query.OrderBy(s => s.Name),
                "name_desc" => query.OrderByDescending(s => s.Name),
                "century" => query.OrderBy(s => s.Century),
                "century_desc" => query.OrderByDescending(s => s.Century),
                "feastday" => query.OrderBy(s => s.FeastDay.HasValue ? s.FeastDay.Value.Month * 100 + s.FeastDay.Value.Day : int.MaxValue),
                "feastday_desc" => query.OrderByDescending(s => s.FeastDay.HasValue ? s.FeastDay.Value.Month * 100 + s.FeastDay.Value.Day : int.MinValue),
                _ => query.OrderBy(s => s.Name)
            };


        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((filters.PageNumber - 1) * filters.PageSize)
            .Take(filters.PageSize)
            .ToListAsync();

        return new PagedResult<Saint>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = filters.PageNumber,
            PageSize = filters.PageSize
        };
    }


    public async Task<Saint?> GetByIdAsync(int id)
    {
        return await context.Saints
            .Include(s => s.Tags)
            .Include(s => s.ReligiousOrder)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Saint?> GetBySlugAsync(string slug)
    {
        return await context.Saints
            .Include(s => s.Tags)
            .Include(s => s.ReligiousOrder)
            .FirstOrDefaultAsync(s => s.Slug == slug);
    }

    public async Task<bool> CreateAsync(Saint newSaint)
    {
        await context.Saints.AddAsync(newSaint);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateAsync(Saint saint)
    {
        context.Saints.Update(saint);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task DeleteAsync(int id)
    {
        var saint = await context.Saints
            .Include(s => s.Tags)
            .Include(s => s.ReligiousOrder)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (saint is not null)
        {
            context.Remove(saint);
            await context.SaveChangesAsync();
        }
    }

    public async Task<IReadOnlyList<string>> GetCountriesAsync()
    {
        return await context.Saints
            .Select(s => s.Country)
            .Distinct()
            .ToListAsync();
    }

    public async Task<int> GetTotalSaintsAsync()
    {
        return await context.Saints.CountAsync();
    }

    public async Task<bool> SlugExistsAsync(string slug)
    {
        return await context.Saints.AnyAsync(s => s.Slug == slug);
    }

    public async Task<Saint?> GetSaintOfTheDayAsync(DateOnly feastDay)
    {
        return await context.Saints
            .Where(s => s.FeastDay.HasValue &&
                        s.FeastDay.Value.Month == feastDay.Month &&
                        s.FeastDay.Value.Day == feastDay.Day &&
                        s.Tags.Any(t => t.Name == "Saint of the Day"))
            .FirstOrDefaultAsync();
    }
}
