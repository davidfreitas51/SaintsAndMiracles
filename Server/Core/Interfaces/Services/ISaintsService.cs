public interface ISaintsService
{
    Task<int?> CreateSaintAsync(NewSaintDto newSaint, string userId);
    Task<bool> UpdateSaintAsync(int id, NewSaintDto updated, string userId);
    Task DeleteSaintAsync(int id, string userId);
}