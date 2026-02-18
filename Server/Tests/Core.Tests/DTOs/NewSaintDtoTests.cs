using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for NewSaintDto validation.
/// Ensures all saint properties are properly validated including name, location, dates, and content.
/// </summary>
public class NewSaintDtoTests : DtoTestBase
{
    private static NewSaintDto CreateValidDto() => new()
    {
        Name = "Saint Francis of Assisi",
        Country = "Italy",
        Century = 12,
        Image = "saints/francis.jpg",
        Description = "Founder of the Franciscan Order.",
        MarkdownContent = "# Saint Francis\nValid text",
        Title = "Confessor",
        FeastDay = new DateOnly(1182, 10, 4),
        PatronOf = "Animals and the environment",
        ReligiousOrderId = 1,
        TagIds = [1, 2],
        Slug = "saint-francis-of-assisi"
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
    public void Should_Pass_MinimalDto()
    {
        var dto = new NewSaintDto
        {
            Name = "St",
            Country = "USA",
            Century = 0,
            Image = "saints/test.jpg",
            Description = "T",
            MarkdownContent = "M"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithUnicodeInternationalNames()
    {
        var dto = CreateValidDto();
        dto.Name = "São João da Cruz";
        dto.Country = "Brésil";

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== NAME VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyName(string? invalidName)
    {
        var dto = CreateValidDto();
        dto.Name = invalidName!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Name));
    }

    [Theory]
    [InlineData("123456")] // Only numbers
    [InlineData("9999")]
    public void Should_Fail_NameOnlyNumbers(string invalidName)
    {
        var dto = CreateValidDto();
        dto.Name = invalidName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Name));
    }

    [Theory]
    [InlineData("Saint<script>")] // XSS attempt
    [InlineData("Saint@#$%")] // Invalid special chars
    public void Should_Fail_InvalidNameFormat(string invalidName)
    {
        var dto = CreateValidDto();
        dto.Name = invalidName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Name));
    }

    [Fact]
    public void Should_Fail_NameTooLong()
    {
        var dto = CreateValidDto();
        dto.Name = new string('A', 151); // Over 150 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Name));
    }

    [Fact]
    public void Should_Pass_NameExactlyMaxLength()
    {
        var dto = CreateValidDto();
        dto.Name = "Saint " + new string('A', 94); // Exactly 100 chars (PersonName max)

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewSaintDto.Name));
    }

    // ==================== COUNTRY VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyCountry(string? invalidCountry)
    {
        var dto = CreateValidDto();
        dto.Country = invalidCountry!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Country));
    }

    [Theory]
    [InlineData("US")] // Too short (< 3 chars)
    [InlineData("It")]
    public void Should_Fail_CountryTooShort(string invalidCountry)
    {
        var dto = CreateValidDto();
        dto.Country = invalidCountry;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Country));
    }

    [Theory]
    [InlineData("Country<script>")] // XSS attempt
    public void Should_Fail_UnsafeCountry(string unsafeCountry)
    {
        var dto = CreateValidDto();
        dto.Country = unsafeCountry;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Country));
    }

    [Fact]
    public void Should_Fail_CountryTooLong()
    {
        var dto = CreateValidDto();
        dto.Country = new string('A', 151); // Over 150 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Country));
    }

    // ==================== CENTURY VALIDATION ====================

    [Theory]
    [InlineData(-20)] // Min boundary
    [InlineData(0)] // Zero
    [InlineData(12)] // Valid middle
    [InlineData(21)] // Max boundary
    public void Should_Pass_ValidCentury(int validCentury)
    {
        var dto = CreateValidDto();
        dto.Century = validCentury;

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewSaintDto.Century));
    }

    [Theory]
    [InlineData(-21)] // Below min
    [InlineData(-50)]
    [InlineData(22)] // Above max
    [InlineData(100)]
    public void Should_Fail_CenturyOutOfRange(int invalidCentury)
    {
        var dto = CreateValidDto();
        dto.Century = invalidCentury;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Century));
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

        AssertInvalidProperty(results, nameof(NewSaintDto.Image));
    }

    [Theory]
    [InlineData("saints/valid-image.jpg")]
    [InlineData("saints/another_image.png")]
    [InlineData("saints/nested/path/image.gif")]
    public void Should_Pass_ValidImagePaths(string validImage)
    {
        var dto = CreateValidDto();
        dto.Image = validImage;

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewSaintDto.Image));
    }

    [Fact]
    public void Should_Fail_PathTraversalInImage()
    {
        var dto = CreateValidDto();
        dto.Image = "../../../etc/passwd"; // Path traversal

        var results = Validate(dto);

        // ImageSource validator rejects path traversal, but error might not specify property name
        AssertInvalid(results);
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

        AssertInvalidProperty(results, nameof(NewSaintDto.Description));
    }

    [Fact]
    public void Should_Fail_DescriptionTooLong()
    {
        var dto = CreateValidDto();
        dto.Description = new string('A', 201); // Over 200 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Description));
    }

    [Theory]
    [InlineData("Description<script>")] // XSS attempt
    [InlineData("Test&nbsp;description")] // HTML entity (not detected by SafeText)
    public void Should_Fail_UnsafeDescription(string unsafeDescription)
    {
        var dto = CreateValidDto();
        dto.Description = unsafeDescription;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Description));
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

        AssertInvalidProperty(results, nameof(NewSaintDto.MarkdownContent));
    }

    [Fact]
    public void Should_Fail_MarkdownContentTooLong()
    {
        var dto = CreateValidDto();
        dto.MarkdownContent = new string('A', 20001); // Over 20000 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.MarkdownContent));
    }

    [Fact]
    public void Should_Pass_MarkdownWithValidFormatting()
    {
        var dto = CreateValidDto();
        dto.MarkdownContent = @"
# Saint Francis
## Early Life
- Born in 1182
- Died in 1226

**Bold text** and *italic text*.

> Quote here

[Link](https://example.com)

```code
var x = 10;
```
";

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewSaintDto.MarkdownContent));
    }

    // ==================== TITLE VALIDATION ====================

    [Fact]
    public void Should_Pass_NullTitle()
    {
        var dto = CreateValidDto();
        dto.Title = null;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Fail_TitleTooLong()
    {
        var dto = CreateValidDto();
        dto.Title = new string('A', 101); // Over 100 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Title));
    }

    // ==================== FEAST DAY VALIDATION ====================

    [Fact]
    public void Should_Pass_NullFeastDay()
    {
        var dto = CreateValidDto();
        dto.FeastDay = null;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(12, 31)]
    [InlineData(6, 15)]
    public void Should_Pass_ValidFeastDay(int month, int day)
    {
        var dto = CreateValidDto();
        dto.FeastDay = new DateOnly(2024, month, day);

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== PATRON OF VALIDATION ====================

    [Fact]
    public void Should_Pass_NullPatronOf()
    {
        var dto = CreateValidDto();
        dto.PatronOf = null;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Fail_PatronOfTooLong()
    {
        var dto = CreateValidDto();
        dto.PatronOf = new string('A', 101); // Over 100 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.PatronOf));
    }

    // ==================== RELIGIOUS ORDER ID VALIDATION ====================

    [Fact]
    public void Should_Pass_NullReligiousOrderId()
    {
        var dto = CreateValidDto();
        dto.ReligiousOrderId = null;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(9999)]
    public void Should_Pass_ValidReligiousOrderId(int orderId)
    {
        var dto = CreateValidDto();
        dto.ReligiousOrderId = orderId;

        var results = Validate(dto);

        AssertValid(results);
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

    [Fact]
    public void Should_Pass_EmptyTagIds()
    {
        var dto = CreateValidDto();
        dto.TagIds = [];

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

        AssertValidProperty(results, nameof(NewSaintDto.TagIds));
    }

    [Fact]
    public void Should_Fail_TooManyTagIds()
    {
        var dto = CreateValidDto();
        dto.TagIds = [1, 2, 3, 4, 5, 6]; // More than 5

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.TagIds));
    }

    // ==================== SLUG VALIDATION ====================

    [Fact]
    public void Should_Pass_NullSlug()
    {
        var dto = CreateValidDto();
        dto.Slug = null;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData("valid-slug")]
    [InlineData("another-valid-slug-123")]
    [InlineData("slug-with-hyphens-only")]
    public void Should_Pass_ValidSlug(string validSlug)
    {
        var dto = CreateValidDto();
        dto.Slug = validSlug;

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewSaintDto.Slug));
    }

    [Theory]
    [InlineData("Invalid Slug With Spaces")]
    [InlineData("slug!@#$%")]
    [InlineData("UPPERCASE")] // SafeSlug requires lowercase
    [InlineData("Título Inválido")]
    public void Should_Fail_InvalidSlug(string invalidSlug)
    {
        var dto = CreateValidDto();
        dto.Slug = invalidSlug;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewSaintDto.Slug));
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_AllRequiredFieldsMissing()
    {
        var dto = new NewSaintDto
        {
            Name = "",
            Country = "",
            Century = 0,
            Image = "",
            Description = "",
            MarkdownContent = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        Assert.True(results.Count >= 4, "Should have at least 4 validation errors");
    }

    [Fact]
    public void Should_Fail_MultipleFieldsInvalid()
    {
        var dto = new NewSaintDto
        {
            Name = "123456", // Only numbers
            Country = "US", // Too short
            Century = 100, // Out of range
            Image = "../../../bad",
            Description = new string('A', 201), // Too long
            MarkdownContent = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        Assert.True(results.Count >= 5, "Should have multiple validation errors");
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_VeryOldSaint()
    {
        var dto = CreateValidDto();
        dto.Century = -20; // Very early Christian era
        dto.FeastDay = new DateOnly(200, 1, 1);

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_ContemporarySaint()
    {
        var dto = CreateValidDto();
        dto.Century = 21; // Modern era
        dto.FeastDay = new DateOnly(2024, 12, 25);

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_AllOptionalFieldsNull()
    {
        var dto = new NewSaintDto
        {
            Name = "Saint Test",
            Country = "TestCountry",
            Century = 12,
            Image = "saints/test.jpg",
            Description = "Test description",
            MarkdownContent = "Test content",
            Title = null,
            FeastDay = null,
            PatronOf = null,
            ReligiousOrderId = null,
            TagIds = null,
            Slug = null
        };

        var results = Validate(dto);

        AssertValid(results);
    }
}
