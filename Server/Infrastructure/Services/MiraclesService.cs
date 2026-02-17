using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class MiraclesService(
    IMiraclesRepository miraclesRepository,
    ITagsRepository tagsRepository,
    IRecentActivityRepository recentActivityRepository,
    IFileStorageService fileStorage,
    ILogger<MiraclesService> logger) : IMiraclesService
{
    private readonly IMiraclesRepository _miraclesRepository = miraclesRepository;
    private readonly ITagsRepository _tagsRepository = tagsRepository;
    private readonly IRecentActivityRepository _recentActivityRepository = recentActivityRepository;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task<int?> CreateMiracleAsync(NewMiracleDto newMiracle, string? userId)
    {
        var slug = _fileStorage.GenerateSlug(newMiracle.Title);

        if (await _miraclesRepository.SlugExistsAsync(slug))
        {
            logger.LogWarning("Miracle creation failed: Slug already exists. Slug={Slug}, UserId={UserId}", slug, userId ?? "Anonymous");
            return null;
        }

        var (markdownPath, imagePath) = await _fileStorage.SaveFilesAsync(
            folderName: "miracles",
            slug: slug,
            markdownContent: newMiracle.MarkdownContent,
            image: newMiracle.Image
        );

        var tags = (newMiracle.TagIds != null && newMiracle.TagIds.Any())
            ? await _tagsRepository.GetByIdsAsync(newMiracle.TagIds)
            : new List<Tag>();

        var miracle = new Miracle
        {
            Title = newMiracle.Title,
            Country = newMiracle.Country,
            Century = newMiracle.Century,
            Image = imagePath ?? "",
            Description = newMiracle.Description,
            MarkdownPath = markdownPath,
            Slug = slug,
            Date = newMiracle.Date,
            LocationDetails = newMiracle.LocationDetails,
            Tags = tags,
        };

        var created = await _miraclesRepository.CreateAsync(miracle);
        if (!created)
        {
            logger.LogWarning("Miracle creation failed in repository. Title={Title}, UserId={UserId}", newMiracle.Title, userId ?? "Anonymous");
            return null;
        }

        await _recentActivityRepository.LogActivityAsync(
            EntityType.Miracle,
            miracle.Id,
            miracle.Title,
            ActivityAction.Created,
            userId
        );

        logger.LogInformation("Miracle created successfully. Id={Id}, Title={Title}, UserId={UserId}", miracle.Id, miracle.Title, userId ?? "Anonymous");
        return miracle.Id;
    }

    public async Task<bool> UpdateMiracleAsync(int id, NewMiracleDto updatedMiracle, string? userId)
    {
        var existingMiracle = await _miraclesRepository.GetByIdAsync(id);
        if (existingMiracle == null)
        {
            logger.LogWarning("Update miracle failed: Miracle not found. Id={Id}, UserId={UserId}", id, userId ?? "Anonymous");
            return false;
        }

        var oldSlug = existingMiracle.Slug;
        var newSlug = _fileStorage.GenerateSlug(updatedMiracle.Title);

        var (markdownPath, imagePath) = await _fileStorage.SaveFilesAsync(
            folderName: "miracles",
            slug: newSlug,
            markdownContent: updatedMiracle.MarkdownContent,
            image: updatedMiracle.Image,
            existingImagePath: existingMiracle.Image
        );

        existingMiracle.Title = updatedMiracle.Title;
        existingMiracle.Country = updatedMiracle.Country;
        existingMiracle.Century = updatedMiracle.Century;
        existingMiracle.Description = updatedMiracle.Description;
        existingMiracle.Slug = newSlug;
        existingMiracle.Date = updatedMiracle.Date;
        existingMiracle.LocationDetails = updatedMiracle.LocationDetails;
        existingMiracle.MarkdownPath = markdownPath;
        existingMiracle.Image = imagePath ?? existingMiracle.Image;

        existingMiracle.Tags = (updatedMiracle.TagIds != null && updatedMiracle.TagIds.Any())
            ? await _tagsRepository.GetByIdsAsync(updatedMiracle.TagIds)
            : new List<Tag>();

        var updated = await _miraclesRepository.UpdateAsync(existingMiracle);
        if (!updated)
        {
            logger.LogWarning("Update miracle failed in repository. Id={Id}, Title={Title}, UserId={UserId}", id, updatedMiracle.Title, userId ?? "Anonymous");
            return false;
        }

        if (!string.Equals(oldSlug, newSlug, StringComparison.OrdinalIgnoreCase))
            await _fileStorage.DeleteFolderAsync("miracles", oldSlug);

        await _recentActivityRepository.LogActivityAsync(
            EntityType.Miracle,
            existingMiracle.Id,
            existingMiracle.Title,
            ActivityAction.Updated,
            userId
        );

        logger.LogInformation("Miracle updated successfully. Id={Id}, Title={Title}, UserId={UserId}", id, updatedMiracle.Title, userId ?? "Anonymous");
        return true;
    }

    public async Task DeleteMiracleAsync(string slug, string? userId)
    {
        var miracle = await _miraclesRepository.GetBySlugAsync(slug);
        if (miracle == null)
        {
            logger.LogWarning("Delete miracle failed: Miracle not found. Slug={Slug}, UserId={UserId}", slug, userId ?? "Anonymous");
            return;
        }

        await _fileStorage.DeleteFolderAsync("miracles", slug);
        await _miraclesRepository.DeleteAsync(miracle.Id);

        await _recentActivityRepository.LogActivityAsync(
            EntityType.Miracle,
            miracle.Id,
            miracle.Title,
            ActivityAction.Deleted,
            userId
        );

        logger.LogInformation("Miracle deleted successfully. Id={Id}, Title={Title}, Slug={Slug}, UserId={UserId}", miracle.Id, miracle.Title, slug, userId ?? "Anonymous");
    }
}