using Core.DTOs;

namespace Tests.Common.Builders;

/// <summary>
/// Fluent builder for creating NewPrayerDto test objects.
/// </summary>
public class NewPrayerDtoBuilder
{
    private string _title = "Hail Mary";
    private string _description = "A traditional Catholic prayer to Mary.";
    private string _image = "/prayers/hail-mary/image.webp";
    private string _markdownContent = "# Hail Mary\n\nHail Mary, full of grace...";
    private List<int>? _tagIds;

    /// <summary>
    /// Creates a default NewPrayerDto with valid test values.
    /// </summary>
    public static NewPrayerDtoBuilder Default()
        => new();

    /// <summary>
    /// Creates a minimal valid NewPrayerDto (only required fields).
    /// </summary>
    public static NewPrayerDtoBuilder Minimal()
        => new NewPrayerDtoBuilder()
            .WithTitle("Prayer")
            .WithDescription("Description")
            .WithImage("/image.webp")
            .WithMarkdownContent("Content");

    /// <summary>
    /// Creates a NewPrayerDto with invalid data (for negative tests).
    /// </summary>
    public static NewPrayerDtoBuilder Invalid()
        => new NewPrayerDtoBuilder()
            .WithTitle("")
            .WithDescription("");

    public NewPrayerDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public NewPrayerDtoBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public NewPrayerDtoBuilder WithImage(string image)
    {
        _image = image;
        return this;
    }

    public NewPrayerDtoBuilder WithMarkdownContent(string content)
    {
        _markdownContent = content;
        return this;
    }

    public NewPrayerDtoBuilder WithTagIds(List<int>? tagIds)
    {
        _tagIds = tagIds;
        return this;
    }

    public NewPrayerDtoBuilder WithTags(params int[] tagIds)
    {
        _tagIds = tagIds.ToList();
        return this;
    }

    /// <summary>
    /// Builds and returns the NewPrayerDto instance.
    /// </summary>
    public NewPrayerDto Build()
    {
        return new NewPrayerDto
        {
            Title = _title,
            Description = _description,
            Image = _image,
            MarkdownContent = _markdownContent,
            TagIds = _tagIds
        };
    }
}
