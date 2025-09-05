using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;

namespace Infrastructure.Services;

public class TagsService(ITagsRepository tagsRepository, IRecentActivityRepository recentActivityRepository) : ITagsService
{
    public async Task<Tag?> CreateTagAsync(NewTagDto dto, string userId)
    {
        var tag = new Tag { Name = dto.Name, TagType = dto.TagType };
        var created = await tagsRepository.CreateAsync(tag);

        if (created)
        {
            await recentActivityRepository.LogActivityAsync(
                EntityType.Tag,
                tag.Id,
                tag.Name,
                ActivityAction.Created,
                userId
            );
            return tag;
        }

        return null;
    }

    public async Task<bool> UpdateTagAsync(int id, NewTagDto dto, string userId)
    {
        var tag = await tagsRepository.GetByIdAsync(id);
        if (tag is null) return false;

        tag.Name = dto.Name;
        tag.TagType = dto.TagType;

        var updated = await tagsRepository.UpdateAsync(tag);
        if (updated)
        {
            await recentActivityRepository.LogActivityAsync(
                EntityType.Tag,
                tag.Id,
                tag.Name,
                ActivityAction.Updated,
                userId
            );
        }

        return updated;
    }

    public async Task<bool> DeleteTagAsync(int id, string userId)
    {
        var tag = await tagsRepository.GetByIdAsync(id);
        if (tag is null) return false;

        await tagsRepository.DeleteAsync(id);

        await recentActivityRepository.LogActivityAsync(
            EntityType.Tag,
            tag.Id,
            tag.Name,
            ActivityAction.Deleted,
            userId
        );

        return true;
    }
}
