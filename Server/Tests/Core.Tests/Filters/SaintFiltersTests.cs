using Core.Models.Filters;
using Xunit;

namespace Core.Tests.Models.Filters;

public class SaintFiltersTests
{
    private SaintFilters CreateValidFilters()
        => new()
        {
            Country = "France",
            Century = "16",
            Search = "Saint Peter",
            FeastMonth = "July",
            ReligiousOrderId = "1",
            TagIds = new List<int> { 1, 2, 3 },
            PageNumber = 1,
            PageSize = 25
        };

    [Fact]
    public void Should_Pass_With_Valid_Filters()
    {
        var filters = CreateValidFilters();
        var results = ModelValidationHelper.Validate(filters);
        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_String_Too_Long()
    {
        var filters = CreateValidFilters();
        filters.Country = new string('A', 51); // Excede MaxLength
        filters.Century = new string('B', 21);
        filters.Search = new string('C', 101);
        filters.FeastMonth = new string('D', 21);
        filters.ReligiousOrderId = new string('E', 21);

        var results = ModelValidationHelper.Validate(filters);

        Assert.Equal(5, results.Count);
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Country must be at most 50 characters"));
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Century must be at most 20 characters"));
        Assert.Contains(results, r => r.ErrorMessage!.Contains("Search must be at most 100 characters"));
        Assert.Contains(results, r => r.ErrorMessage!.Contains("FeastMonth must be at most 20 characters"));
        Assert.Contains(results, r => r.ErrorMessage!.Contains("ReligiousOrderId must be at most 20 characters"));
    }

    [Fact]
    public void Should_Fail_When_TagIds_Exceed_MaxItems()
    {
        var filters = CreateValidFilters();
        filters.TagIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }; // 11 itens

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("TagIds can contain at most 10 items", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_PageNumber_Out_Of_Range()
    {
        var filters = CreateValidFilters();
        filters.PageNumber = 0; // < 1

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("PageNumber must be between 1 and 1000", results[0].ErrorMessage);

        filters.PageNumber = 1001; // > 1000
        results = ModelValidationHelper.Validate(filters);
        Assert.Single(results);
        Assert.Contains("PageNumber must be between 1 and 1000", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_PageSize_Out_Of_Range()
    {
        var filters = CreateValidFilters();
        filters.PageSize = 0; // < 1

        var results = ModelValidationHelper.Validate(filters);

        Assert.Single(results);
        Assert.Contains("PageSize must be between 1 and 100", results[0].ErrorMessage);

        filters.PageSize = 101; // > 100
        results = ModelValidationHelper.Validate(filters);
        Assert.Single(results);
        Assert.Contains("PageSize must be between 1 and 100", results[0].ErrorMessage);
    }
}
