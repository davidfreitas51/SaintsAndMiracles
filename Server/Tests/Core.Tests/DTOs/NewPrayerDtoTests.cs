using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for NewPrayerDto validation.
/// Ensures prayer properties are properly validated including title, content, and images.
/// </summary>
public class NewPrayerDtoTests : DtoTestBase
{
    private static NewPrayerDto CreateValidDto() => new()
    {
        Title = "Our Father",
        Description = "The Lord's Prayer taught by Jesus",
        MarkdownContent = "# Our Father\n\nOur Father who art in heaven...",
        Image = "prayers/our-father.jpg"
    };

    // ==================== VALID DTO ====================

    [Fact]
    public void Should_Pass_ValidDto()
    {
        var dto = CreateValidDto();

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithOptionalTagIds()
    {
        var dto = CreateValidDto();
        dto.TagIds = [1, 2, 3];

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== TITLE VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyTitle(string? invalidTitle)
    {
        var dto = CreateValidDto();
        dto.Title = invalidTitle!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.Title));
    }

    [Theory]
    [InlineData("AB")] // Too short (< 3 chars)
    [InlineData("A")]
    public void Should_Fail_TitleTooShort(string shortTitle)
    {
        var dto = CreateValidDto();
        dto.Title = shortTitle;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.Title));
    }

    [Theory]
    [InlineData("Prayer 123")] // With spaces and numbers
    [InlineData("999 Angels")]
    public void Should_Pass_TitleWithNumbers(string validTitle)
    {
        var dto = CreateValidDto();
        dto.Title = validTitle;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData("Prayer<script>")]
    [InlineData("Title&nbsp;Test")]
    public void Should_Fail_UnsafeTitle(string unsafeTitle)
    {
        var dto = CreateValidDto();
        dto.Title = unsafeTitle;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.Title));
    }

    [Fact]
    public void Should_Fail_TitleTooLong()
    {
        var dto = CreateValidDto();
        dto.Title = new string('A', 151); // Over 150 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.Title));
    }

    // ==================== DESCRIPTION VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_EmptyDescription(string? invalidDescription)
    {
        var dto = CreateValidDto();
        dto.Description = invalidDescription!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.Description));
    }

    [Theory]
    [InlineData("Description<script>")]
    [InlineData("Test&amp;Description")]
    public void Should_Fail_UnsafeDescription(string unsafeDescription)
    {
        var dto = CreateValidDto();
        dto.Description = unsafeDescription;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.Description));
    }

    [Fact]
    public void Should_Fail_DescriptionTooLong()
    {
        var dto = CreateValidDto();
        dto.Description = new string('A', 201); // Over 200 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.Description));
    }

    // ==================== MARKDOWN CONTENT VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_EmptyMarkdownContent(string? invalidContent)
    {
        var dto = CreateValidDto();
        dto.MarkdownContent = invalidContent!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.MarkdownContent));
    }

    [Fact]
    public void Should_Pass_MarkdownWithFormatting()
    {
        var dto = CreateValidDto();
        dto.MarkdownContent = @"
# Prayer Title
## Section
- Item 1
- Item 2

**Bold** and *italic*

> Quote
";

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewPrayerDto.MarkdownContent));
    }

    [Fact]
    public void Should_Fail_MarkdownContentTooLong()
    {
        var dto = CreateValidDto();
        dto.MarkdownContent = new string('A', 20001); // Over 20000 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.MarkdownContent));
    }

    // ==================== IMAGE VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_EmptyImage(string? invalidImage)
    {
        var dto = CreateValidDto();
        dto.Image = invalidImage!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.Image));
    }

    [Theory]
    [InlineData("prayers/valid-image.jpg")]
    [InlineData("prayers/another_image.png")]
    [InlineData("prayers/nested/path/image.gif")]
    public void Should_Pass_ValidImagePaths(string validImage)
    {
        var dto = CreateValidDto();
        dto.Image = validImage;

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewPrayerDto.Image));
    }

    // ==================== TAG IDS VALIDATION ====================

    [Fact]
    public void Should_Pass_NullTagIds()
    {
        var dto = CreateValidDto();
        dto.TagIds = null;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)] // Max allowed
    public void Should_Pass_ValidTagIdCount(int count)
    {
        var dto = CreateValidDto();
        dto.TagIds = Enumerable.Range(1, count).ToList();

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewPrayerDto.TagIds));
    }

    [Fact]
    public void Should_Fail_TooManyTagIds()
    {
        var dto = CreateValidDto();
        dto.TagIds = [1, 2, 3, 4, 5, 6]; // More than 5

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewPrayerDto.TagIds));
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_AllRequiredFieldsMissing()
    {
        var dto = new NewPrayerDto
        {
            Title = "",
            Description = "",
            MarkdownContent = "",
            Image = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        Assert.True(results.Count >= 4, "Should have at least 4 validation errors");
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_MinimumValidLengths()
    {
        var dto = new NewPrayerDto
        {
            Title = "Ave", // Exactly 3 chars
            Description = "P", // Minimum 1 char
            MarkdownContent = "M", // Minimum 1 char
            Image = "prayers/p.jpg"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_TitleWithUnicode()
    {
        var dto = CreateValidDto();
        dto.Title = "Молитва Отче наш"; // Russian

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewPrayerDto.Title));
    }
}
