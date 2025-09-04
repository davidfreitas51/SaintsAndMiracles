using Core.DTOs;

namespace Core.Interfaces.Services;

public interface IMiraclesService
{
    Task<int?> CreateMiracleAsync(NewMiracleDto newMiracle, string? userId);
    Task<bool> UpdateMiracleAsync(int id, NewMiracleDto updatedMiracle, string? userId);
    Task DeleteMiracleAsync(string slug, string? userId);
    Task<(string markdownPath, string? imagePath)> SaveFilesAsync(NewMiracleDto miracleDto, string slug);
    Task<(string markdownPath, string? imagePath)> UpdateFilesAsync(NewMiracleDto miracleDto, string slug);
}
