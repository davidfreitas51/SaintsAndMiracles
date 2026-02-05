using System.Text.RegularExpressions;
using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Services;

public class MiraclesService(
    IHostEnvironment env,
    IMiraclesRepository miraclesRepository,
    IRecentActivityRepository recentActivityRepository,
    ITagsRepository tagsRepository) : IMiraclesService
{
    public async Task<int?> CreateMiracleAsync(NewMiracleDto newMiracle, string? userId)
    {
        var slug = GenerateSlug(newMiracle.Title);
        if (await miraclesRepository.SlugExistsAsync(slug))
            return null;

        var (markdownPath, imagePath) = await SaveFilesAsync(newMiracle, slug);

        var tags = new List<Tag>();
        if (newMiracle.TagIds != null && newMiracle.TagIds.Any())
            tags = await tagsRepository.GetByIdsAsync(newMiracle.TagIds);

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

        var created = await miraclesRepository.CreateAsync(miracle);
        if (!created) return null;

        await recentActivityRepository.LogActivityAsync(
            EntityType.Miracle,
            miracle.Id,
            miracle.Title,
            ActivityAction.Created,
            userId
        );

        return miracle.Id;
    }

    public async Task<bool> UpdateMiracleAsync(int id, NewMiracleDto updatedMiracle, string? userId)
    {
        var existingMiracle = await miraclesRepository.GetByIdAsync(id);
        if (existingMiracle == null)
            return false;

        var slug = GenerateSlug(updatedMiracle.Title);
        var (markdownPath, imagePath) = await UpdateFilesAsync(updatedMiracle, slug);

        existingMiracle.Title = updatedMiracle.Title;
        existingMiracle.Country = updatedMiracle.Country;
        existingMiracle.Century = updatedMiracle.Century;
        existingMiracle.Description = updatedMiracle.Description;
        existingMiracle.Slug = slug;
        existingMiracle.Date = updatedMiracle.Date;
        existingMiracle.LocationDetails = updatedMiracle.LocationDetails;

        if (!string.IsNullOrWhiteSpace(imagePath))
            existingMiracle.Image = imagePath;
        if (!string.IsNullOrWhiteSpace(markdownPath))
            existingMiracle.MarkdownPath = markdownPath;

        if (updatedMiracle.TagIds != null && updatedMiracle.TagIds.Any())
            existingMiracle.Tags = await tagsRepository.GetByIdsAsync(updatedMiracle.TagIds);
        else
            existingMiracle.Tags = new List<Tag>();

        var updated = await miraclesRepository.UpdateAsync(existingMiracle);
        if (!updated) return false;

        await recentActivityRepository.LogActivityAsync(
            EntityType.Miracle,
            existingMiracle.Id,
            existingMiracle.Title,
            ActivityAction.Updated,
            userId
        );

        return true;
    }

    public async Task DeleteMiracleAsync(string slug, string? userId)
    {
        var miracle = await miraclesRepository.GetBySlugAsync(slug);
        if (miracle == null) return;

        var wwwroot = Path.Combine(env.ContentRootPath, "wwwroot");
        var miracleFolder = Path.Combine(wwwroot, "miracles", slug);
        if (Directory.Exists(miracleFolder))
            Directory.Delete(miracleFolder, recursive: true);

        await miraclesRepository.DeleteAsync(miracle.Id);

        await recentActivityRepository.LogActivityAsync(
            EntityType.Miracle,
            miracle.Id,
            miracle.Title,
            ActivityAction.Deleted,
            userId
        );
    }

    private string GenerateSlug(string title)
    {
        return Regex.Replace(title.ToLower(), @"[^a-z0-9]+", "-").Trim('-');
    }

    public async Task<(string markdownPath, string? imagePath)> SaveFilesAsync(NewMiracleDto miracleDto, string slug)
    {
        var wwwroot = Path.Combine(env.ContentRootPath, "wwwroot");
        var miracleFolder = Path.Combine(wwwroot, "miracles", slug);
        Directory.CreateDirectory(miracleFolder);

        var markdownPath = Path.Combine(miracleFolder, "markdown.md");
        await File.WriteAllTextAsync(markdownPath, miracleDto.MarkdownContent);
        var relativeMarkdownPath = $"/miracles/{slug}/markdown.md";

        string? relativeImagePath = null;
        if (!string.IsNullOrWhiteSpace(miracleDto.Image) && miracleDto.Image.StartsWith("data:image/"))
        {
            var match = Regex.Match(miracleDto.Image, @"data:image/(?<type>.+?);base64,(?<data>.+)");
            if (match.Success)
            {
                var extension = match.Groups["type"].Value;
                var base64Data = match.Groups["data"].Value;
                var imageBytes = Convert.FromBase64String(base64Data);

                var imagePath = Path.Combine(miracleFolder, $"image.{extension}");
                await File.WriteAllBytesAsync(imagePath, imageBytes);

                relativeImagePath = $"/miracles/{slug}/image.{extension}";
            }
        }

        return (relativeMarkdownPath, relativeImagePath);
    }

    public async Task<(string markdownPath, string? imagePath)> UpdateFilesAsync(NewMiracleDto miracleDto, string slug)
    {
        return await SaveFilesAsync(miracleDto, slug);
    }
}
