using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for NewMiracleDto validation.
/// Ensures miracle properties are validated including location, date, and content.
/// </summary>
public class NewMiracleDtoTests : DtoTestBase
{
    private static NewMiracleDto CreateValidDto() => new()
    {
        Title = "Miracle of Lourdes",
        Country = "France",
        Century = 19,
        Image = "miracles/lourdes.jpg",
        Description = "Description of the miracle at Lourdes",
        MarkdownContent = "# Miracle\n\nDetailed content here...",
        Date = "1858",
        LocationDetails = "Lourdes, France",
        TagIds = [1, 2, 3]
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
        var dto = new NewMiracleDto
        {
            Title = "Mir",
            Country = "USA",
            Century = 0,
            Image = "miracles/test.jpg",
            Description = "D",
            MarkdownContent = "M"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== TITLE VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_EmptyTitle(string? invalidTitle)
    {
        var dto = CreateValidDto();
        dto.Title = invalidTitle!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.Title));
    }

    [Theory]
    [InlineData("AB")] // Too short
    public void Should_Fail_InvalidTitle(string invalidTitle)
    {
        var dto = CreateValidDto();
        dto.Title = invalidTitle;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.Title));
    }

    [Theory]
    [InlineData("ABC 123")] // Valid - has letters and numbers
    public void Should_Pass_TitleWithNumbers(string validTitle)
    {
        var dto = CreateValidDto();
        dto.Title = validTitle;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("Title&nbsp;Test")]
    public void Should_Fail_UnsafeTitle(string unsafeTitle)
    {
        var dto = CreateValidDto();
        dto.Title = unsafeTitle;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.Title));
    }

    // ==================== COUNTRY VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Should_Fail_EmptyCountry(string? invalidCountry)
    {
        var dto = CreateValidDto();
        dto.Country = invalidCountry!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.Country));
    }

    [Theory]
    [InlineData("US")] // Too short (< 3 chars)
    public void Should_Fail_CountryTooShort(string shortCountry)
    {
        var dto = CreateValidDto();
        dto.Country = shortCountry;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.Country));
    }

    // ==================== CENTURY VALIDATION ====================

    [Theory]
    [InlineData(-20)] // Min
    [InlineData(0)]
    [InlineData(21)] // Max
    public void Should_Pass_ValidCentury(int validCentury)
    {
        var dto = CreateValidDto();
        dto.Century = validCentury;

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewMiracleDto.Century));
    }

    [Theory]
    [InlineData(-21)] // Below min
    [InlineData(22)] // Above max
    public void Should_Fail_CenturyOutOfRange(int invalidCentury)
    {
        var dto = CreateValidDto();
        dto.Century = invalidCentury;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.Century));
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

        AssertInvalidProperty(results, nameof(NewMiracleDto.Description));
    }

    [Theory]
    [InlineData("<b>HTML</b>")]
    [InlineData("Test&amp;Valid")]
    public void Should_Fail_UnsafeDescription(string unsafeDescription)
    {
        var dto = CreateValidDto();
        dto.Description = unsafeDescription;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.Description));
    }

    // ==================== DATE VALIDATION ====================

    [Fact]
    public void Should_Pass_NullDate()
    {
        var dto = CreateValidDto();
        dto.Date = null;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData("<script>1858</script>")]
    [InlineData("1858&nbsp;AD")]
    public void Should_Fail_UnsafeDate(string unsafeDate)
    {
        var dto = CreateValidDto();
        dto.Date = unsafeDate;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.Date));
    }

    // ==================== LOCATION DETAILS VALIDATION ====================

    [Fact]
    public void Should_Pass_NullLocationDetails()
    {
        var dto = CreateValidDto();
        dto.LocationDetails = null;

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("Location&amp;Miracle")]
    public void Should_Fail_UnsafeLocationDetails(string unsafeLocation)
    {
        var dto = CreateValidDto();
        dto.LocationDetails = unsafeLocation;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.LocationDetails));
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
    public void Should_Fail_TooManyTagIds()
    {
        var dto = CreateValidDto();
        dto.TagIds = [1, 2, 3, 4, 5, 6]; // More than 5

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewMiracleDto.TagIds));
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_AllRequiredFieldsMissing()
    {
        var dto = new NewMiracleDto
        {
            Title = "",
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

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_AllOptionalFieldsNull()
    {
        var dto = new NewMiracleDto
        {
            Title = "Test Miracle",
            Country = "TestCountry",
            Century = 12,
            Image = "miracles/test.jpg",
            Description = "Test description",
            MarkdownContent = "Test content",
            Date = null,
            LocationDetails = null,
            TagIds = null
        };

        var results = Validate(dto);

        AssertValid(results);
    }
}
