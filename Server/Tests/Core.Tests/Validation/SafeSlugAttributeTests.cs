using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class SafeSlugAttributeTests
{
    private class TestDto
    {
        [SafeSlug]
        public string? Slug { get; set; }
    }

    [Fact]
    public void Should_Pass_When_Value_IsNull()
    {
        var dto = new TestDto { Slug = null };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Value_IsEmpty()
    {
        var dto = new TestDto { Slug = "" };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Slug), results[0].MemberNames);
        Assert.Contains("cannot be empty", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Pass_When_Slug_IsValid()
    {
        var dto = new TestDto { Slug = "saint-john-paul-ii" };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Slug_Starts_With_Dash()
    {
        var dto = new TestDto { Slug = "-invalid-slug" };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Slug), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Slug_Ends_With_Dash()
    {
        var dto = new TestDto { Slug = "invalid-slug-" };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Slug), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Slug_Contains_Double_Dash()
    {
        var dto = new TestDto { Slug = "invalid--slug" };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Slug), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Slug_Has_Uppercase()
    {
        var dto = new TestDto { Slug = "Invalid-slug" };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Slug), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Slug_Contains_Special_Characters()
    {
        var dto = new TestDto { Slug = "slug@123" };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Slug), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Slug_Contains_Spaces()
    {
        var dto = new TestDto { Slug = "invalid slug" };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Slug), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Slug_IsTooLong()
    {
        var dto = new TestDto
        {
            Slug = new string('a', 151)
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Slug), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }
}