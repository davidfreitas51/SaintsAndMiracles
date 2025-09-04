namespace Core.Models;

public class RecentActivity
{
    public int Id { get; set; }
    public string EntityName { get; set; } = null!;
    public int EntityId { get; set; }
    public string DisplayName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
}