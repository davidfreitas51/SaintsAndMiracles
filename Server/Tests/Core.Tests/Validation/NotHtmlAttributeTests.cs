using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class NotHtmlAttributeTests
{
    private class TestDto
    {
        [NotHtml]
        public string? Text { get; set; }

        [NotHtml(ErrorMessage = "Custom error message")]
        public string? CustomText { get; set; }

        [NotHtml]
        public object? NotAString { get; set; }
    }

    [Fact]
    public void Should_Pass_When_Value_IsNull()
    {
        var dto = new TestDto
        {
            Text = null
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_When_Text_HasNoHtml()
    {
        var dto = new TestDto
        {
            Text = "Hello world!"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Text_Contains_Html()
    {
        var dto = new TestDto
        {
            Text = "<b>Hello</b>"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Text), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Use_Custom_ErrorMessage_When_Provided()
    {
        var dto = new TestDto
        {
            CustomText = "<i>test</i>"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.CustomText), results[0].MemberNames);
        Assert.Equal("Custom error message", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Value_IsNotString()
    {
        var dto = new TestDto
        {
            NotAString = 123
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains("Invalid data type", results[0].ErrorMessage);
    }
}
