public interface IFileStorageService
{
    Task DeleteFolderAsync(string folderName, string slug);
    Task<(string markdownPath, string? imagePath)> SaveFilesAsync(
        string folderName,
        string slug,
        string markdownContent,
        string? image,
        string? existingImagePath = null);
    string GenerateSlug(string name);
    Task RenameFolderIfNeededAsync(string folderName, string oldSlug, string newSlug);
}