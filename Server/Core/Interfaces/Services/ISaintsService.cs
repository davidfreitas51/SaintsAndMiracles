public interface ISaintsService
{
    Task<int?> CreateSaintAsync(NewSaintDto newSaint, string userId);
    Task<bool> UpdateSaintAsync(int id, NewSaintDto updatedSaint, string userId);
    Task DeleteSaintAsync(int id, string userId);
    Task<(string markdownPath, string? imagePath)> SaveFilesAsync(NewSaintDto saintDto, string slug);
    Task<(string markdownPath, string? imagePath)> UpdateFilesAsync(NewSaintDto saintDto, string slug);
}
