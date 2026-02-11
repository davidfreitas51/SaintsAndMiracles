using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace Infrastructure.Services;

public class FileStorageService(IHostEnvironment env) : IFileStorageService
{
    private readonly string _wwwroot = Path.Combine(env.ContentRootPath, "wwwroot");

    public string GenerateSlug(string name)
        => Regex.Replace(name.ToLower(), @"[^a-z0-9]+", "-").Trim('-');

    public async Task DeleteFolderAsync(string folderName, string slug)
    {
        var folder = Path.Combine(_wwwroot, folderName, slug);
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);

        await Task.CompletedTask;
    }

    public async Task RenameFolderIfNeededAsync(string folderName, string oldSlug, string newSlug)
    {
        if (string.Equals(oldSlug, newSlug, StringComparison.OrdinalIgnoreCase))
            return;

        var baseFolder = Path.Combine(_wwwroot, folderName);
        var oldFolder = Path.Combine(baseFolder, oldSlug);
        var newFolder = Path.Combine(baseFolder, newSlug);

        if (Directory.Exists(oldFolder))
            Directory.Move(oldFolder, newFolder);
        else
            Directory.CreateDirectory(newFolder);

        await Task.CompletedTask;
    }

    public async Task<(string markdownPath, string? imagePath)> SaveFilesAsync(
        string folderName,
        string slug,
        string markdownContent,
        string? image,
        string? existingImagePath = null)
    {
        var folder = Path.Combine(_wwwroot, folderName, slug);
        Directory.CreateDirectory(folder);

        var markdownPath = Path.Combine(folder, "markdown.md");
        await File.WriteAllTextAsync(markdownPath, markdownContent);
        var relativeMarkdownPath = $"/{folderName}/{slug}/markdown.md";

        string? relativeImagePath = null;

        if (!string.IsNullOrWhiteSpace(image))
        {
            if (image.StartsWith("data:image/"))
            {
                if (!string.IsNullOrWhiteSpace(existingImagePath))
                    TryDeleteFile(Path.Combine(_wwwroot, existingImagePath.TrimStart('/')));

                var match = Regex.Match(image, @"data:image/(?<type>.+?);base64,(?<data>.+)");
                if (match.Success)
                {
                    var bytes = Convert.FromBase64String(match.Groups["data"].Value);

                    using var img = Image.Load(bytes);
                    var webpPath = Path.Combine(folder, "image.webp");
                    await img.SaveAsync(webpPath, new WebpEncoder { Quality = 80 });

                    relativeImagePath = $"/{folderName}/{slug}/image.webp";
                }
            }
            else
            {
                var oldImageFullPath = Path.Combine(_wwwroot, image.TrimStart('/'));
                if (File.Exists(oldImageFullPath))
                {
                    var newImagePath = Path.Combine(folder, Path.GetFileName(oldImageFullPath));
                    if (!string.Equals(oldImageFullPath, newImagePath, StringComparison.OrdinalIgnoreCase))
                        File.Copy(oldImageFullPath, newImagePath, overwrite: true);

                    relativeImagePath = $"/{folderName}/{slug}/{Path.GetFileName(oldImageFullPath)}";
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(existingImagePath))
        {
            var oldImageFullPath = Path.Combine(_wwwroot, existingImagePath.TrimStart('/'));
            if (File.Exists(oldImageFullPath))
            {
                var newImagePath = Path.Combine(folder, Path.GetFileName(oldImageFullPath));
                if (!string.Equals(oldImageFullPath, newImagePath, StringComparison.OrdinalIgnoreCase))
                    File.Move(oldImageFullPath, newImagePath);

                relativeImagePath = $"/{folderName}/{slug}/{Path.GetFileName(oldImageFullPath)}";
            }
        }

        return (relativeMarkdownPath, relativeImagePath);
    }

    private void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
        }
    }
}