namespace Core.Interfaces.Services;

public interface IMiraclesService
{
    Task<int?> CreateMiracleAsync(NewMiracleDto newMiracle, string? userId);
    Task<bool> UpdateMiracleAsync(int id, NewMiracleDto updatedMiracle, string? userId);
    Task DeleteMiracleAsync(string slug, string? userId);
}
