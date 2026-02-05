using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services;

public class SaintsService : ISaintsService
{
    private readonly ISaintsRepository saintsRepository;
    private readonly ITagsRepository tagsRepository;
    private readonly IReligiousOrdersRepository religiousOrdersRepository;
    private readonly IRecentActivityRepository recentActivityRepository;
    private readonly UserManager<AppUser> userManager;
    private readonly IFileStorageService fileStorage;

    public SaintsService(
        ISaintsRepository saintsRepository,
        ITagsRepository tagsRepository,
        IReligiousOrdersRepository religiousOrdersRepository,
        IRecentActivityRepository recentActivityRepository,
        UserManager<AppUser> userManager,
        IFileStorageService fileStorage)
    {
        this.saintsRepository = saintsRepository;
        this.tagsRepository = tagsRepository;
        this.religiousOrdersRepository = religiousOrdersRepository;
        this.recentActivityRepository = recentActivityRepository;
        this.userManager = userManager;
        this.fileStorage = fileStorage;
    }

    public async Task<int?> CreateSaintAsync(NewSaintDto newSaint, string userId)
    {
        var slug = fileStorage.GenerateSlug(newSaint.Name);
        if (await saintsRepository.SlugExistsAsync(slug))
            return null;

        var (markdownPath, imagePath) = await fileStorage.SaveFilesAsync("saints", slug, newSaint.MarkdownContent, newSaint.Image);

        var tags = (newSaint.TagIds != null && newSaint.TagIds.Any())
            ? await tagsRepository.GetByIdsAsync(newSaint.TagIds)
            : new List<Tag>();

        ReligiousOrder? order = null;
        if (newSaint.ReligiousOrderId.HasValue)
            order = await religiousOrdersRepository.GetByIdAsync(newSaint.ReligiousOrderId.Value);

        var user = await userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var saint = new Saint
        {
            Name = newSaint.Name,
            Country = newSaint.Country,
            Century = newSaint.Century,
            MarkdownPath = markdownPath,
            Image = imagePath ?? "",
            Description = newSaint.Description,
            Slug = slug,
            Title = newSaint.Title,
            FeastDay = newSaint.FeastDay,
            PatronOf = newSaint.PatronOf,
            ReligiousOrder = order,
            Tags = tags
        };

        var created = await saintsRepository.CreateAsync(saint);
        if (!created) return null;

        await recentActivityRepository.LogActivityAsync(EntityType.Saint, saint.Id, saint.Name, ActivityAction.Created, userId);
        return saint.Id;
    }

    public async Task<bool> UpdateSaintAsync(int id, NewSaintDto updatedSaint, string userId)
    {
        var saint = await saintsRepository.GetByIdAsync(id);
        if (saint == null) return false;

        var oldSlug = saint.Slug;
        var newSlug = fileStorage.GenerateSlug(updatedSaint.Name);

        string? oldImagePath = saint.Image;

        var (markdownPath, imagePath) = await fileStorage.SaveFilesAsync(
            folderName: "saints",
            slug: newSlug,
            markdownContent: updatedSaint.MarkdownContent,
            image: updatedSaint.Image,
            existingImagePath: oldImagePath
        );

        saint.Name = updatedSaint.Name;
        saint.Slug = newSlug;
        saint.MarkdownPath = markdownPath;
        saint.Image = imagePath ?? oldImagePath;
        saint.Country = updatedSaint.Country;
        saint.Century = updatedSaint.Century;
        saint.Description = updatedSaint.Description;
        saint.Title = updatedSaint.Title;
        saint.FeastDay = updatedSaint.FeastDay;
        saint.PatronOf = updatedSaint.PatronOf;
        saint.ReligiousOrderId = updatedSaint.ReligiousOrderId;
        saint.Tags = (updatedSaint.TagIds != null && updatedSaint.TagIds.Any())
            ? await tagsRepository.GetByIdsAsync(updatedSaint.TagIds)
            : new List<Tag>();

        var updatedResult = await saintsRepository.UpdateAsync(saint);
        if (!updatedResult) return false;

        if (!string.Equals(oldSlug, newSlug, StringComparison.OrdinalIgnoreCase))
        {
            await fileStorage.DeleteFolderAsync("saints", oldSlug);
        }

        await recentActivityRepository.LogActivityAsync(
            EntityType.Saint, saint.Id, saint.Name, ActivityAction.Updated, userId
        );

        return true;
    }

    public async Task DeleteSaintAsync(int id, string userId)
    {
        var saint = await saintsRepository.GetByIdAsync(id);
        if (saint == null) return;

        await fileStorage.DeleteFolderAsync("saints", saint.Slug);

        await saintsRepository.DeleteAsync(saint.Id);
        await recentActivityRepository.LogActivityAsync(EntityType.Saint, saint.Id, saint.Name, ActivityAction.Deleted, userId);
    }
}