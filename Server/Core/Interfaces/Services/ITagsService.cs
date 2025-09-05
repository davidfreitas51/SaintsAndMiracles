using Core.DTOs;
using Core.Models;

namespace Core.Interfaces.Services;

public interface ITagsService
{
    Task<Tag?> CreateTagAsync(NewTagDto dto, string userId);
    Task<bool> UpdateTagAsync(int id, NewTagDto dto, string userId);
    Task<bool> DeleteTagAsync(int id, string userId);
}
