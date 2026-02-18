using Core.DTOs;

namespace Tests.Common.Builders;

public class NewMiracleDtoBuilder
{
    private string _title = "Sample Miracle";
    private string _description = "A miraculous event description";
    private string _markdownContent = "## Miracle Details\n\nDescription of the miracle.";
    private string _country = "Italy";
    private string _image = "miracle.webp";
    private int _century = 13;

    public static NewMiracleDtoBuilder Default() => new();

    public NewMiracleDtoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public NewMiracleDtoBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public NewMiracleDtoBuilder WithMarkdownContent(string markdownContent)
    {
        _markdownContent = markdownContent;
        return this;
    }

    public NewMiracleDtoBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    public NewMiracleDtoBuilder WithImage(string image)
    {
        _image = image;
        return this;
    }

    public NewMiracleDtoBuilder WithCentury(int century)
    {
        _century = century;
        return this;
    }

    public NewMiracleDto Build()
    {
        return new NewMiracleDto
        {
            Title = _title,
            Description = _description,
            MarkdownContent = _markdownContent,
            Country = _country,
            Image = _image,
            Century = _century
        };
    }

    public static NewMiracleDtoBuilder Minimal()
    {
        return Default();
    }

    public static NewMiracleDtoBuilder Invalid()
    {
        return new NewMiracleDtoBuilder
        {
            _title = "",
            _description = "",
            _markdownContent = ""
        };
    }
}
