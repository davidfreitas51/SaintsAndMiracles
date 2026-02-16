using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class PrayersService(
    IPrayersRepository prayersRepository,
    ITagsRepository tagsRepository,
    IRecentActivityRepository recentActivityRepository,
    IFileStorageService fileStorage,
    ILogger<PrayersService> logger) : IPrayersService
{
    private readonly IPrayersRepository _prayersRepository = prayersRepository;
    private readonly ITagsRepository _tagsRepository = tagsRepository;
    private readonly IRecentActivityRepository _recentActivityRepository = recentActivityRepository;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task<int?> CreatePrayerAsync(NewPrayerDto newPrayer, string? userId)
    {
        var slug = _fileStorage.GenerateSlug(newPrayer.Title);
        if (await _prayersRepository.SlugExistsAsync(slug))
        {
            logger.LogWarning("Prayer creation failed: Slug already exists. Slug={Slug}, UserId={UserId}", slug, userId ?? "Anonymous");
            return null;
        }

        var (markdownPath, imagePath) = await _fileStorage.SaveFilesAsync(
            folderName: "prayers",
            slug: slug,
            markdownContent: newPrayer.MarkdownContent,
            image: newPrayer.Image
        );

        var tags = (newPrayer.TagIds != null && newPrayer.TagIds.Any())
            ? await _tagsRepository.GetByIdsAsync(newPrayer.TagIds)
            : new List<Tag>();

        var prayer = new Prayer
        {
            Title = newPrayer.Title,
            Description = newPrayer.Description,
            Slug = slug,
            MarkdownPath = markdownPath,
            Image = imagePath ?? "",
            Tags = tags
        };

        var created = await _prayersRepository.CreateAsync(prayer);
        if (!created)
        {
            logger.LogWarning("Prayer creation failed in repository. Title={Title}, UserId={UserId}", newPrayer.Title, userId ?? "Anonymous");
            return null;
        }

        await _recentActivityRepository.LogActivityAsync(
            EntityType.Prayer,
            prayer.Id,
            prayer.Title,
            ActivityAction.Created,
            userId
        );

        logger.LogInformation("Prayer created successfully. Id={Id}, Title={Title}, UserId={UserId}", prayer.Id, prayer.Title, userId ?? "Anonymous");
        return prayer.Id;
    }

    public async Task<bool> UpdatePrayerAsync(int id, NewPrayerDto updatedPrayer, string? userId)
    {
        var existingPrayer = await _prayersRepository.GetByIdAsync(id);
        if (existingPrayer == null)
        {
            logger.LogWarning("Update prayer failed: Prayer not found. Id={Id}, UserId={UserId}", id, userId ?? "Anonymous");
            return false;
        }

        var oldSlug = existingPrayer.Slug;
        var newSlug = _fileStorage.GenerateSlug(updatedPrayer.Title);

        var (markdownPath, imagePath) = await _fileStorage.SaveFilesAsync(
            folderName: "prayers",
            slug: newSlug,
            markdownContent: updatedPrayer.MarkdownContent,
            image: updatedPrayer.Image,
            existingImagePath: existingPrayer.Image
        );

        existingPrayer.Title = updatedPrayer.Title;
        existingPrayer.Description = updatedPrayer.Description;
        existingPrayer.Slug = newSlug;
        existingPrayer.MarkdownPath = markdownPath;
        existingPrayer.Image = imagePath ?? existingPrayer.Image;

        existingPrayer.Tags = (updatedPrayer.TagIds != null && updatedPrayer.TagIds.Any())
            ? await _tagsRepository.GetByIdsAsync(updatedPrayer.TagIds)
            : new List<Tag>();

        var updated = await _prayersRepository.UpdateAsync(existingPrayer);
        if (!updated)
        {
            logger.LogWarning("Update prayer failed in repository. Id={Id}, Title={Title}, UserId={UserId}", id, updatedPrayer.Title, userId ?? "Anonymous");
            return false;
        }

        if (!string.Equals(oldSlug, newSlug, StringComparison.OrdinalIgnoreCase))
            await _fileStorage.DeleteFolderAsync("prayers", oldSlug);

        await _recentActivityRepository.LogActivityAsync(
            EntityType.Prayer,
            existingPrayer.Id,
            existingPrayer.Title,
            ActivityAction.Updated,
            userId
        );

        logger.LogInformation("Prayer updated successfully. Id={Id}, Title={Title}, UserId={UserId}", id, updatedPrayer.Title, userId ?? "Anonymous");
        return true;
    }

    public async Task DeletePrayerAsync(string slug, string? userId)
    {
        var prayer = await _prayersRepository.GetBySlugAsync(slug);
        if (prayer == null)
        {
            logger.LogWarning("Delete prayer failed: Prayer not found. Slug={Slug}, UserId={UserId}", slug, userId ?? "Anonymous");
            return;
        }

        await _fileStorage.DeleteFolderAsync("prayers", slug);
        await _prayersRepository.DeleteAsync(prayer);

        await _recentActivityRepository.LogActivityAsync(
            EntityType.Prayer,
            prayer.Id,
            prayer.Title,
            ActivityAction.Deleted,
            userId
        );

        logger.LogInformation("Prayer deleted successfully. Id={Id}, Title={Title}, Slug={Slug}, UserId={UserId}", prayer.Id, prayer.Title, slug, userId ?? "Anonymous");
    }
}