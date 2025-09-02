namespace Core.Models;

public class Prayer
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Image { get; set; }
    public required string MarkdownPath { get; set; }
    public required string Slug { get; set; }
    public List<Tag> Tags { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

}
