using Core.DTOs;

namespace Tests.Common.Builders;

/// <summary>
/// Fluent builder for creating NewSaintDto test objects.
/// </summary>
public class NewSaintDtoBuilder
{
    private string _name = "Saint Francis of Assisi";
    private string _country = "Italy";
    private int _century = 12;
    private string _image = "/saints/francis-of-assisi/image.webp";
    private string _description = "Founder of the Franciscan order, patron of animals.";
    private string _markdownContent = "# Saint Francis\n\nContent about Saint Francis.";
    private string? _title = "The Poor Man of Assisi";
    private DateOnly? _feastDay = new DateOnly(1, 10, 4);
    private string? _patronOf = "Animals and Ecology";
    private int? _religiousOrderId;
    private List<int>? _tagIds;
    private string? _slug = "saint-francis-of-assisi";

    /// <summary>
    /// Creates a default NewSaintDto with valid test values.
    /// </summary>
    public static NewSaintDtoBuilder Default()
        => new();

    /// <summary>
    /// Creates a minimal valid NewSaintDto (only required fields).
    /// </summary>
    public static NewSaintDtoBuilder Minimal()
        => new NewSaintDtoBuilder()
            .WithName("Saint Name")
            .WithCountry("Country")
            .WithCentury(5)
            .WithImage("/image.webp")
            .WithDescription("Description")
            .WithMarkdownContent("Content")
            .WithTitle(null)
            .WithFeastDay(null)
            .WithPatronOf(null)
            .WithSlug(null);

    /// <summary>
    /// Creates a NewSaintDto with invalid data (for negative tests).
    /// </summary>
    public static NewSaintDtoBuilder Invalid()
        => new NewSaintDtoBuilder()
            .WithName("")
            .WithCountry("");

    public NewSaintDtoBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public NewSaintDtoBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    public NewSaintDtoBuilder WithCentury(int century)
    {
        _century = century;
        return this;
    }

    public NewSaintDtoBuilder WithImage(string image)
    {
        _image = image;
        return this;
    }

    public NewSaintDtoBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public NewSaintDtoBuilder WithMarkdownContent(string content)
    {
        _markdownContent = content;
        return this;
    }

    public NewSaintDtoBuilder WithTitle(string? title)
    {
        _title = title;
        return this;
    }

    public NewSaintDtoBuilder WithFeastDay(DateOnly? feastDay)
    {
        _feastDay = feastDay;
        return this;
    }

    public NewSaintDtoBuilder WithPatronOf(string? patronOf)
    {
        _patronOf = patronOf;
        return this;
    }

    public NewSaintDtoBuilder WithReligiousOrderId(int? orderId)
    {
        _religiousOrderId = orderId;
        return this;
    }

    public NewSaintDtoBuilder WithTagIds(List<int>? tagIds)
    {
        _tagIds = tagIds;
        return this;
    }

    public NewSaintDtoBuilder WithTags(params int[] tagIds)
    {
        _tagIds = tagIds.ToList();
        return this;
    }

    public NewSaintDtoBuilder WithSlug(string? slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Builds and returns the NewSaintDto instance.
    /// </summary>
    public NewSaintDto Build()
    {
        return new NewSaintDto
        {
            Name = _name,
            Country = _country,
            Century = _century,
            Image = _image,
            Description = _description,
            MarkdownContent = _markdownContent,
            Title = _title,
            FeastDay = _feastDay,
            PatronOf = _patronOf,
            ReligiousOrderId = _religiousOrderId,
            TagIds = _tagIds,
            Slug = _slug
        };
    }
}
