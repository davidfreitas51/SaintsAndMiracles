using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class SafeTokenAttributeTests
{
    private class TestDto
    {
        [SafeToken]
        public string? Token { get; set; }
    }

    [Fact]
    public void Should_Pass_When_Value_IsNull()
    {
        var dto = new TestDto { Token = null };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Value_IsEmpty()
    {
        var dto = new TestDto { Token = "" };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Single(results);
        Assert.Contains(nameof(dto.Token), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Pass_When_Value_IsValid()
    {
        string token = "ABCDEFGHIJKLMNOPQRSTUVWXYZ012345";
        var dto = new TestDto { Token = token };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("Invalid$TokenWith32CharsLongHere!!")]
    [InlineData("TokenWith@SignAnd32CharsExactlyAAA")]
    public void Should_Fail_When_Value_HasInvalidCharacters(string token)
    {
        var dto = new TestDto { Token = token };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Single(results);
        Assert.Contains(nameof(dto.Token), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Theory]
    [InlineData("TokenWith SpaceInsideAndValidLength1234")]
    [InlineData("TokenWith\tTabCharacterAndValidLengthAB")]
    [InlineData("TokenWith\nNewlineCharacterAndValidLen")]
    public void Should_Fail_When_Value_HasWhitespaceOrControl(string token)
    {
        var dto = new TestDto { Token = token };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Single(results);
        Assert.Contains(nameof(dto.Token), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Value_IsNotString()
    {
        var dto = new object();
        var attr = new SafeTokenAttribute();
        var context = new ValidationContext(dto) { MemberName = "Token" };
        var result = attr.GetValidationResult(123, context);
        Assert.NotNull(result);
        Assert.Contains("invalid format", result!.ErrorMessage);
    }
}
