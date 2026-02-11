using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;

namespace Infrastructure.Services;

public class PrayersService(
    IPrayersRepository prayersRepository,
    ITagsRepository tagsRepository,
    IRecentActivityRepository recentActivityRepository,
    IFileStorageService fileStorage) : IPrayersService
{
    private readonly IPrayersRepository _prayersRepository = prayersRepository;
    private readonly ITagsRepository _tagsRepository = tagsRepository;
    private readonly IRecentActivityRepository _recentActivityRepository = recentActivityRepository;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task<int?> CreatePrayerAsync(NewPrayerDto newPrayer, string? userId)
    {
        var slug = _fileStorage.GenerateSlug(newPrayer.Title);
        if (await _prayersRepository.SlugExistsAsync(slug))
            return null;

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
        if (!created) return null;

        await _recentActivityRepository.LogActivityAsync(
            EntityType.Prayer,
            prayer.Id,
            prayer.Title,
            ActivityAction.Created,
            userId
        );

        return prayer.Id;
    }

    public async Task<bool> UpdatePrayerAsync(int id, NewPrayerDto updatedPrayer, string? userId)
    {
        var existingPrayer = await _prayersRepository.GetByIdAsync(id);
        if (existingPrayer == null) return false;

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
        if (!updated) return false;

        if (!string.Equals(oldSlug, newSlug, StringComparison.OrdinalIgnoreCase))
            await _fileStorage.DeleteFolderAsync("prayers", oldSlug);

        await _recentActivityRepository.LogActivityAsync(
            EntityType.Prayer,
            existingPrayer.Id,
            existingPrayer.Title,
            ActivityAction.Updated,
            userId
        );

        return true;
    }

    public async Task DeletePrayerAsync(string slug, string? userId)
    {
        var prayer = await _prayersRepository.GetBySlugAsync(slug);
        if (prayer == null) return;

        await _fileStorage.DeleteFolderAsync("prayers", slug);
        await _prayersRepository.DeleteAsync(prayer);

        await _recentActivityRepository.LogActivityAsync(
            EntityType.Prayer,
            prayer.Id,
            prayer.Title,
            ActivityAction.Deleted,
            userId
        );
    }
}