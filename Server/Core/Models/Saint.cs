namespace Core.Models;

public class Saint
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Country { get; set; }
    public required int Century { get; set; }
    public required string Image { get; set; }
    public required string Description { get; set; }
    public required string MarkdownPath { get; set; }
    public string? Title { get; set; }
    public DateOnly? FeastDay { get; set; }
    public string? PatronOf { get; set; }
    public int? ReligiousOrderId { get; set; }
    public ReligiousOrder? ReligiousOrder { get; set; }
    public required string Slug { get; set; }
    public List<Tag> Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
