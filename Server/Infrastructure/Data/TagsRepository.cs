using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.Filters;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class TagsRepository(DataContext context, ICacheService cacheService, ILogger<TagsRepository> logger) : ITagsRepository
{
    public async Task<PagedResult<Tag>> GetAllAsync(EntityFilters filters)
    {
        var typePart = filters.Type.HasValue ? filters.Type.Value.ToString() : "";
        var cacheKey = cacheService.BuildKey(
            "tag",
            $"list_page{filters.Page}_size{filters.PageSize}_search{filters.Search}_type{typePart}",
            incrementVersion: false
        );

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var query = context.Tags.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filters.Search))
                    query = query.Where(t => t.Name.Contains(filters.Search));

                if (filters.Type.HasValue)
                {
                    var tagTypeEnum = (TagType)filters.Type.Value;
                    query = query.Where(t => t.TagType == tagTypeEnum);
                }

                var total = await query.CountAsync();

                var pageNumber = filters.Page <= 0 ? 1 : filters.Page;
                var pageSize = filters.PageSize <= 0 ? 10 : filters.PageSize;

                var items = await query
                    .OrderBy(t => t.Name)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagedResult<Tag>
                {
                    Items = items,
                    TotalCount = total,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        );

        return result ?? new PagedResult<Tag>();
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        var cacheKey = cacheService.BuildKey("tag", $"id{id}", incrementVersion: false);
        return await cacheService.GetOrSetAsync(
            cacheKey,
            () => context.Tags.FindAsync(id).AsTask()
        );
    }

    public async Task<List<Tag>> GetByIdsAsync(List<int> ids)
    {
        var cacheKey = cacheService.BuildKey("tag", $"ids_{string.Join("-", ids)}", incrementVersion: false);

        var result = await cacheService.GetOrSetAsync(
            cacheKey,
            () => context.Tags.Where(t => ids.Contains(t.Id)).ToListAsync()
        );

        return result ?? new List<Tag>();
    }

    public async Task<bool> CreateAsync(Tag tag)
    {
        logger.LogInformation("Creating tag: Name={Name}, Type={TagType}", tag.Name, tag.TagType);
        context.Tags.Add(tag);
        var created = await context.SaveChangesAsync() > 0;

        if (created)
        {
            InvalidateCaches(tag);
            logger.LogInformation("Tag created: Id={Id}, Name={Name}, Type={TagType}", tag.Id, tag.Name, tag.TagType);
        }
        else
        {
            logger.LogWarning("Tag creation failed to save. Name={Name}, Type={TagType}", tag.Name, tag.TagType);
        }

        return created;
    }

    public async Task<bool> UpdateAsync(Tag tag)
    {
        logger.LogInformation("Updating tag: Id={Id}, Name={Name}, Type={TagType}", tag.Id, tag.Name, tag.TagType);
        context.Tags.Update(tag);
        var updated = await context.SaveChangesAsync() > 0;

        if (updated)
        {
            InvalidateCaches(tag);
            logger.LogInformation("Tag updated: Id={Id}, Name={Name}, Type={TagType}", tag.Id, tag.Name, tag.TagType);
        }
        else
        {
            logger.LogWarning("Tag update failed to save. Id={Id}", tag.Id);
        }

        return updated;
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await context.Tags.FindAsync(id);
        if (tag is null)
        {
            logger.LogWarning("Delete failed: Tag not found. Id={Id}", id);
            return;
        }

        context.Tags.Remove(tag);
        var deleted = await context.SaveChangesAsync() > 0;

        if (deleted)
        {
            InvalidateCaches(tag);
            logger.LogInformation("Tag deleted from database. Id={Id}, Name={Name}, Type={TagType}", id, tag.Name, tag.TagType);
        }
        else
        {
            logger.LogWarning("Tag deletion failed to save. Id={Id}", id);
        }
    }

    private void InvalidateCaches(Tag tag)
    {
        switch (tag.TagType)
        {
            case TagType.Prayer:
                cacheService.GetNextVersion("prayer");
                break;
            case TagType.Saint:
                cacheService.GetNextVersion("saint");
                break;
            case TagType.Miracle:
                cacheService.GetNextVersion("miracle");
                break;
        }

        cacheService.GetNextVersion("tag");

        logger.LogInformation("Tag caches invalidated. Id={Id}, Name={Name}, Type={TagType}", tag.Id, tag.Name, tag.TagType);
    }
}
