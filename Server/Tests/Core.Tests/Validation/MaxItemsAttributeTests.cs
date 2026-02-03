using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class MaxItemsAttributeTests
{
    private class TestDto
    {
        [MaxItems(3)]
        public List<int>? Numbers { get; set; }

        [MaxItems(2)]
        public object? NotACollection { get; set; }
    }

    [Fact]
    public void Should_Pass_When_Collection_IsNull()
    {
        var dto = new TestDto
        {
            Numbers = null
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_When_Collection_IsWithinLimit()
    {
        var dto = new TestDto
        {
            Numbers = new List<int> { 1, 2, 3 }
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Collection_ExceedsLimit()
    {
        var dto = new TestDto
        {
            Numbers = new List<int> { 1, 2, 3, 4 }
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Numbers), results[0].MemberNames);
        Assert.Contains("must contain at most 3 items", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Value_IsNotCollection()
    {
        var dto = new TestDto
        {
            NotACollection = new object()
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.NotACollection), results[0].MemberNames);
        Assert.Contains("must be a collection", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Pass_When_Collection_IsEmpty()
    {
        var dto = new TestDto
        {
            Numbers = new List<int>()
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }
}
