using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class TagsService(ITagsRepository tagsRepository, IRecentActivityRepository recentActivityRepository, ILogger<TagsService> logger) : ITagsService
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
            logger.LogInformation("Tag created successfully. Id={Id}, Name={Name}, Type={Type}, UserId={UserId}", tag.Id, tag.Name, dto.TagType, userId);
            return tag;
        }

        logger.LogWarning("Tag creation failed in repository. Name={Name}, Type={Type}, UserId={UserId}", dto.Name, dto.TagType, userId);
        return null;
    }

    public async Task<bool> UpdateTagAsync(int id, NewTagDto dto, string userId)
    {
        var tag = await tagsRepository.GetByIdAsync(id);
        if (tag is null)
        {
            logger.LogWarning("Update tag failed: Tag not found. Id={Id}, UserId={UserId}", id, userId);
            return false;
        }

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
            logger.LogInformation("Tag updated successfully. Id={Id}, Name={Name}, Type={Type}, UserId={UserId}", id, dto.Name, dto.TagType, userId);
        }
        else
        {
            logger.LogWarning("Tag update failed in repository. Id={Id}, Name={Name}, Type={Type}, UserId={UserId}", id, dto.Name, dto.TagType, userId);
        }

        return updated;
    }

    public async Task<bool> DeleteTagAsync(int id, string userId)
    {
        var tag = await tagsRepository.GetByIdAsync(id);
        if (tag is null)
        {
            logger.LogWarning("Delete tag failed: Tag not found. Id={Id}, UserId={UserId}", id, userId);
            return false;
        }

        await tagsRepository.DeleteAsync(id);

        await recentActivityRepository.LogActivityAsync(
            EntityType.Tag,
            tag.Id,
            tag.Name,
            ActivityAction.Deleted,
            userId
        );

        logger.LogInformation("Tag deleted successfully. Id={Id}, Name={Name}, UserId={UserId}", id, tag.Name, userId);
        return true;
    }
}
