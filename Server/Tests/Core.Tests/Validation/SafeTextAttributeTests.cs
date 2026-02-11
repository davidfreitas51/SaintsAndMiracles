using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class SafeTextAttributeTests
{
    private class TestDto
    {
        [SafeText]
        public string? Text { get; set; }

        [SafeText(ErrorMessage = "Custom error message")]
        public string? CustomText { get; set; }

        [SafeText]
        public object? NotAString { get; set; }
    }

    [Fact]
    public void Should_Pass_When_Value_IsNull()
    {
        var dto = new TestDto { Text = null };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_When_Text_Is_Valid()
    {
        var dto = new TestDto { Text = "Just a normal text" };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Text_Contains_HtmlTags()
    {
        var dto = new TestDto { Text = "<b>bold</b>" };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Text), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Text_Contains_HtmlEntities()
    {
        var dto = new TestDto { Text = "Hello &amp; World" };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Text), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Use_Custom_ErrorMessage_When_Provided()
    {
        var dto = new TestDto { CustomText = "<i>test</i>" };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.CustomText), results[0].MemberNames);
        Assert.Equal("Custom error message", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Value_IsNotString()
    {
        var dto = new TestDto { NotAString = 123 };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains("SafeText can only be applied to string fields", results[0].ErrorMessage);
    }
}
