using Core.Models.Filters;
using Xunit;

namespace Core.Tests.Models.Filters;

/// <summary>
/// Tests for PrayerFilters validation.
/// Ensures search, pagination, and tag filtering work correctly.
/// </summary>
public class PrayerFiltersTests
{
    private static PrayerFilters CreateValidFilters() => new()
    {
        Search = "Prayer for peace",
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
        var filters = new PrayerFilters();

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

    // ==================== SEARCH VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Pass_EmptySearch(string emptySearch)
    {
        var filters = CreateValidFilters();
        filters.Search = emptySearch;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("Prayer")]
    [InlineData("P")]
    [InlineData("A")]
    public void Should_Pass_ShortSearch(string shortSearch)
    {
        var filters = CreateValidFilters();
        filters.Search = shortSearch;

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("Prayer for protection and peace")]
    [InlineData("Saint Mary Mother of God grace blessing")]
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
        Assert.Contains("Search must be at most 100 characters", results[0].ErrorMessage!);
    }

    [Theory]
    [InlineData("Prayer<script>")]
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
    [InlineData("Prayer with 123 numbers")]
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
        Assert.Contains("TagIds can contain at most 10 items", results[0].ErrorMessage!);
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
        Assert.Contains("PageNumber must be between 1 and 1000", results[0].ErrorMessage!);
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
        Assert.Contains("PageNumber must be between 1 and 1000", results[0].ErrorMessage!);
    }

    // ==================== PAGE SIZE VALIDATION ====================

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
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
        Assert.Contains("PageSize must be between 1 and 100", results[0].ErrorMessage!);
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
        Assert.Contains("PageSize must be between 1 and 100", results[0].ErrorMessage!);
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_MultipleFieldsInvalid()
    {
        var filters = new PrayerFilters
        {
            Search = new string('A', 101),
            PageNumber = 0,
            PageSize = 101,
            TagIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }
        };

        var results = ModelValidationHelper.Validate(filters);

        Assert.Equal(4, results.Count);
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_AllFieldsAtBoundaries()
    {
        var filters = new PrayerFilters
        {
            Search = new string('A', 100),
            PageNumber = 1,
            PageSize = 100,
            TagIds = Enumerable.Range(1, 10).ToList()
        };

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_SearchWithUnicodeCharacters()
    {
        var filters = CreateValidFilters();
        filters.Search = "Prayer for 日本語 العربية Ελληνικά";

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_OrderByPropertyIgnored()
    {
        var filters = CreateValidFilters();
        filters.OrderBy = PrayerOrderBy.TitleDesc; // Test OrderBy enum

        var results = ModelValidationHelper.Validate(filters);

        Assert.Empty(results);
    }
}
