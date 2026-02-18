using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Tests for PersonNameAttribute validation.
/// Ensures person names meet length and character requirements.
/// </summary>
public class PersonNameAttributeTests : ValidationTestBase
{
    private class TestModel
    {
        [PersonName]
        public string? Name { get; set; }
    }

    // ==================== VALID NAMES ====================

    [Theory]
    [InlineData("John")]
    [InlineData("José da Silva")]
    [InlineData("Mary Jane")]
    [InlineData("François Müller")]
    [InlineData("María José")]
    [InlineData("O'Brien")]
    [InlineData("Jean-Pierre")]
    [InlineData("Anne-Marie")]
    [InlineData("João Paulo II")]
    [InlineData("Nuño")]
    public void Should_Pass_ValidNames(string validName)
    {
        var model = new TestModel { Name = validName };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_Null()
    {
        var model = new TestModel { Name = null };

        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== LENGTH VALIDATION ====================

    [Theory]
    [InlineData("J")]
    [InlineData("A")]
    public void Should_Fail_TooShort(string shortName)
    {
        var model = new TestModel { Name = shortName };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Name), "between");
    }

    [Fact]
    public void Should_Pass_MinLength()
    {
        var model = new TestModel { Name = "Jo" }; // Exactly 2 chars

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Fail_TooLong()
    {
        var model = new TestModel { Name = new string('A', 101) };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Name), "between");
    }

    [Fact]
    public void Should_Pass_MaxLength()
    {
        var model = new TestModel { Name = new string('A', 100) }; // Exactly 100 chars

        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== INVALID CHARACTERS ====================

    [Theory]
    [InlineData("John123")]
    [InlineData("Mary@Doe")]
    [InlineData("!Invalid")]
    [InlineData("Peter#Smith")]
    [InlineData("User$Name")]
    [InlineData("Test%Name")]
    [InlineData("Name&Name")]
    [InlineData("Name*Name")]
    [InlineData("Name(Test)")]
    [InlineData("Name=Value")]
    [InlineData("Name+Sign")]
    [InlineData("Name[bracket]")]
    [InlineData("Name{brace}")]
    [InlineData("Name|Pipe")]
    [InlineData("Name\\Backslash")]
    [InlineData("Name/Slash")]
    [InlineData("Name<Less")]
    [InlineData("Name>Greater")]
    public void Should_Fail_InvalidCharacters(string invalidName)
    {
        var model = new TestModel { Name = invalidName };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Name), "invalid format");
    }

    [Theory]
    [InlineData("Name\tTab")]
    [InlineData("Name\nNewline")]
    [InlineData("Name\rReturn")]
    [InlineData("Name\u0001Control")]
    public void Should_Fail_ControlCharacters(string nameWithControl)
    {
        var model = new TestModel { Name = nameWithControl };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Name), "invalid format");
    }

    // ==================== EDGE CASES ====================

    [Theory]
    [InlineData("Name  Double  Space")]
    [InlineData("Name   Triple   Space")]
    public void Should_Pass_MultipleSpaces(string nameWithSpaces)
    {
        var model = new TestModel { Name = nameWithSpaces };

        var results = Validate(model);

        AssertValid(results);
    }

    [Theory]
    [InlineData(" LeadingSpace")]
    [InlineData("TrailingSpace ")]
    public void Should_Pass_LeadingOrTrailingSpaces(string nameWithSpaces)
    {
        var model = new TestModel { Name = nameWithSpaces };

        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== TYPE VALIDATION ====================

    [Fact]
    public void Should_Fail_NonStringType()
    {
        var attr = new PersonNameAttribute();
        var context = new ValidationContext(new { }) { MemberName = "TestField" };

        var result = attr.GetValidationResult(123, context);

        Assert.NotNull(result);
        Assert.Contains("must be a string", result!.ErrorMessage);
    }
}
