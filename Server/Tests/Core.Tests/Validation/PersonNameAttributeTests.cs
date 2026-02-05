using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class PersonNameAttributeTests
{
    private class TestDto
    {
        [PersonName]
        public string? Name { get; set; }
    }


    [Fact]
    public void Should_Pass_When_Name_IsValid()
    {
        var dto = new TestDto { Name = "José da Silva" };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_When_Name_IsNull()
    {
        var dto = new TestDto { Name = null };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Name_IsTooShort()
    {
        var dto = new TestDto { Name = "J" };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Name), results[0].MemberNames);
        Assert.Contains("between", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Name_IsTooLong()
    {
        var dto = new TestDto { Name = new string('A', 101) };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Name), results[0].MemberNames);
        Assert.Contains("between", results[0].ErrorMessage);
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("Mary@Doe")]
    [InlineData("!Invalid")]
    [InlineData("Peter#Smith")]
    public void Should_Fail_When_Name_Has_InvalidCharacters(string invalidName)
    {
        var dto = new TestDto { Name = invalidName };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Name), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Value_IsNotString()
    {
        var attr = new PersonNameAttribute();
        var context = new ValidationContext(new { }) { MemberName = "TestField" };

        var result = attr.GetValidationResult(123, context);

        Assert.NotNull(result);
        Assert.Contains("must be a string", result!.ErrorMessage);
    }
}
