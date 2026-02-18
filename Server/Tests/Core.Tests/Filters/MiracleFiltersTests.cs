using Core.Models.Filters;
using Xunit;

namespace Core.Tests.Models.Filters;

/// <summary>
/// Tests for MiracleFilters validation.
/// Ensures search, country/century filtering, and pagination work correctly.
/// </summary>
public class MiracleFiltersTests
{
    private static MiracleFilters CreateValidFilters() => new()
    {
        Country = "France",
        Century = "19",
        Search = "Miracle of Lourdes",
        TagIds = new List<int> { 1, 2, 3 },
        PageNumber = 1,
        PageSize = 25
    };

    // ==================== VALID FILTERS ====================

    [Fact]
    public void Should_Pass_ValidFilters()
    {
        var filters = CreateValidFilters();

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_WithDefaultValues()
    {
        var filters = new MiracleFilters();

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_WithEmptyStringFields()
    {
        var filters = new MiracleFilters
        {
            Country = "",
            Century = "",
            Search = ""
        };

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_WithNullTagIds()
    {
        var filters = CreateValidFilters();
        filters.TagIds = null;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    // ==================== COUNTRY VALIDATION ====================

    [Fact]
    public void Should_Pass_EmptyCountry()
    {
        var filters = CreateValidFilters();
        filters.Country = "";

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_CountryWithSpaces()
    {
        var filters = CreateValidFilters();
        filters.Country = "   ";

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("France")]
    [InlineData("Italy")]
    [InlineData("Spain")]
    [InlineData("Germany")]
    public void Should_Pass_ValidCountry(string validCountry)
    {
        var filters = CreateValidFilters();
        filters.Country = validCountry;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_CountryAtMaxLength()
    {
        var filters = CreateValidFilters();
        filters.Country = new string('A', 50); // Exactly 50 chars

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_CountryExceedsMaxLength()
    {
        var filters = CreateValidFilters();
        filters.Country = new string('A', 51); // Over 50 chars

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("50", results[0].ErrorMessage!);
    }

    [Theory]
    [InlineData("Country<script>")]
    [InlineData("France&nbsp;Paris")]
    [InlineData("Country&amp;Region")]
    public void Should_Fail_UnsafeCountry(string unsafeCountry)
    {
        var filters = CreateValidFilters();
        filters.Country = unsafeCountry;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
    }

    [Theory]
    [InlineData("Country & Region")]
    [InlineData("France 2024")]
    public void Should_Pass_CountryWithSpecialCharacters(string validCountry)
    {
        var filters = CreateValidFilters();
        filters.Country = validCountry;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    // ==================== CENTURY VALIDATION ====================

    [Fact]
    public void Should_Pass_EmptyCentury()
    {
        var filters = CreateValidFilters();
        filters.Century = "";

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_CenturyWithOnlySpaces()
    {
        var filters = CreateValidFilters();
        filters.Century = "   ";

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("Century must be up to 2 digits", results[0].ErrorMessage!);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("5")]
    [InlineData("9")]
    public void Should_Pass_SingleDigitCentury(string validCentury)
    {
        var filters = CreateValidFilters();
        filters.Century = validCentury;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("10")]
    [InlineData("15")]
    [InlineData("19")]
    [InlineData("20")]
    [InlineData("99")]
    public void Should_Pass_TwoDigitCentury(string validCentury)
    {
        var filters = CreateValidFilters();
        filters.Century = validCentury;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("100")] // 3 digits
    [InlineData("999")]
    [InlineData("1234")]
    public void Should_Fail_CenturyTooManyDigits(string invalidCentury)
    {
        var filters = CreateValidFilters();
        filters.Century = invalidCentury;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("Century must be up to 2 digits", results[0].ErrorMessage!);
    }

    [Theory]
    [InlineData("12a")] // Contains letters
    [InlineData("a12")]
    [InlineData("1a")]
    public void Should_Fail_CenturyWithLetters(string invalidCentury)
    {
        var filters = CreateValidFilters();
        filters.Century = invalidCentury;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("Century must be up to 2 digits", results[0].ErrorMessage!);
    }

    [Theory]
    [InlineData("1-2")] // Special characters
    [InlineData("1.2")]
    [InlineData("1 2")] // Space
    public void Should_Fail_CenturyWithNonDigitCharacters(string invalidCentury)
    {
        var filters = CreateValidFilters();
        filters.Century = invalidCentury;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("Century must be up to 2 digits", results[0].ErrorMessage!);
    }

    // ==================== SEARCH VALIDATION ====================

    [Fact]
    public void Should_Pass_EmptySearch()
    {
        var filters = CreateValidFilters();
        filters.Search = "";

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_SearchWithSpaces()
    {
        var filters = CreateValidFilters();
        filters.Search = "   ";

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("Miracle")]
    [InlineData("Miracle of Lourdes")]
    [InlineData("Search for miracles")]
    public void Should_Pass_ValidSearch(string validSearch)
    {
        var filters = CreateValidFilters();
        filters.Search = validSearch;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_SearchAtMaxLength()
    {
        var filters = CreateValidFilters();
        filters.Search = new string('A', 100); // Exactly 100 chars

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_SearchExceedsMaxLength()
    {
        var filters = CreateValidFilters();
        filters.Search = new string('A', 101); // Over 100 chars

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("100", results[0].ErrorMessage!);
    }

    [Theory]
    [InlineData("Miracle<script>")]
    [InlineData("Search&nbsp;Text")]
    [InlineData("Query&amp;Result")]
    public void Should_Fail_UnsafeSearch(string unsafeSearch)
    {
        var filters = CreateValidFilters();
        filters.Search = unsafeSearch;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
    }

    [Theory]
    [InlineData("Search & Find")]
    [InlineData("Miracle with 123 numbers")]
    public void Should_Pass_SearchWithSpecialCharacters(string validSearch)
    {
        var filters = CreateValidFilters();
        filters.Search = validSearch;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    // ==================== TAG IDS VALIDATION ====================

    [Fact]
    public void Should_Pass_EmptyTagIds()
    {
        var filters = CreateValidFilters();
        filters.TagIds = new List<int>();

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_SingleTag()
    {
        var filters = CreateValidFilters();
        filters.TagIds = new List<int> { 1 };

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_MaxTags()
    {
        var filters = CreateValidFilters();
        filters.TagIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }; // Exactly 10

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_ExceedsMaxTags()
    {
        var filters = CreateValidFilters();
        filters.TagIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }; // 11 items

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("10", results[0].ErrorMessage!);
    }

    [Fact]
    public void Should_Fail_TooManyTags()
    {
        var filters = CreateValidFilters();
        filters.TagIds = Enumerable.Range(1, 25).ToList(); // 25 items

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
    }

    // ==================== PAGE NUMBER VALIDATION ====================

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(100)]
    [InlineData(1000)] // Max
    public void Should_Pass_ValidPageNumber(int validPageNumber)
    {
        var filters = CreateValidFilters();
        filters.PageNumber = validPageNumber;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Should_Fail_PageNumberTooLow(int invalidPageNumber)
    {
        var filters = CreateValidFilters();
        filters.PageNumber = invalidPageNumber;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("between 1 and 1000", results[0].ErrorMessage!);
    }

    [Theory]
    [InlineData(1001)]
    [InlineData(5000)]
    [InlineData(int.MaxValue)]
    public void Should_Fail_PageNumberTooHigh(int invalidPageNumber)
    {
        var filters = CreateValidFilters();
        filters.PageNumber = invalidPageNumber;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("between 1 and 1000", results[0].ErrorMessage!);
    }

    // ==================== PAGE SIZE VALIDATION ====================

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)] // Default
    [InlineData(50)]
    [InlineData(100)] // Max
    public void Should_Pass_ValidPageSize(int validPageSize)
    {
        var filters = CreateValidFilters();
        filters.PageSize = validPageSize;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void Should_Fail_PageSizeTooLow(int invalidPageSize)
    {
        var filters = CreateValidFilters();
        filters.PageSize = invalidPageSize;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("between 1 and 100", results[0].ErrorMessage!);
    }

    [Theory]
    [InlineData(101)]
    [InlineData(1000)]
    [InlineData(int.MaxValue)]
    public void Should_Fail_PageSizeTooHigh(int invalidPageSize)
    {
        var filters = CreateValidFilters();
        filters.PageSize = invalidPageSize;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("between 1 and 100", results[0].ErrorMessage!);
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_MultipleFieldsInvalid()
    {
        var filters = new MiracleFilters
        {
            Country = new string('A', 51),
            Century = "999",
            Search = new string('B', 101),
            PageNumber = 0,
            PageSize = 101,
            TagIds = Enumerable.Range(1, 11).ToList()
        };

        var results = ModelValidationHelper.Validate(filters);

        Assert.Equal(6, results.Count);
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_AllFieldsAtBoundaries()
    {
        var filters = new MiracleFilters
        {
            Country = new string('A', 50),
            Century = "99",
            Search = new string('B', 100),
            PageNumber = 1,
            PageSize = 100,
            TagIds = Enumerable.Range(1, 10).ToList()
        };

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_WithUnicodeCharacters()
    {
        var filters = new MiracleFilters
        {
            Country = "日本 Italia España",
            Search = "Miracolo José María"
        };

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_CenturyZero()
    {
        var filters = CreateValidFilters();
        filters.Century = "0";

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_OrderByPropertyIgnored()
    {
        var filters = CreateValidFilters();
        filters.OrderBy = MiracleOrderBy.CenturyDesc; // Test OrderBy enum

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }
}
