using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class SafePathAttributeTests
{
    private class TestDto
    {
        [SafePath]
        public string? Path { get; set; }
    }

    [Fact]
    public void Should_Pass_When_Value_IsNull()
    {
        var dto = new TestDto { Path = null };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_When_Value_IsEmpty()
    {
        var dto = new TestDto { Path = "" };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Empty(results);
    }

    [Fact]
    public void Should_Pass_When_Path_IsRelative_AndValid()
    {
        var dto = new TestDto { Path = "images/photo.png" };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Path_IsAbsolute()
    {
        var dto = new TestDto { Path = "/usr/local/file.txt" };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Single(results);
        Assert.Contains(nameof(dto.Path), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Path_Contains_DotDot()
    {
        var dto = new TestDto { Path = "../secret.txt" };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Single(results);
        Assert.Contains(nameof(dto.Path), results[0].MemberNames);
        Assert.Contains("must not contain '..'", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Path_Contains_Colon()
    {
        var dto = new TestDto { Path = "C:/folder/file.txt" };
        var results = ModelValidationHelper.Validate(dto);
        Assert.Single(results);
        Assert.Contains(nameof(dto.Path), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Path_Has_Invalid_Characters()
    {
        var invalidChars = System.IO.Path.GetInvalidPathChars();
        var invalidPath = "folder/" + new string(invalidChars) + "/file.txt";

        var dto = new TestDto { Path = invalidPath };
        var results = ModelValidationHelper.Validate(dto);

        Assert.Single(results);
        Assert.Contains(nameof(dto.Path), results[0].MemberNames);
        Assert.Contains("invalid format", results[0].ErrorMessage);
    }

    [Fact]
    public void Should_Fail_When_Value_IsNotString()
    {
        var dto = new TestDto { Path = null! };
        var results = ModelValidationHelper.Validate(dto);
    }
}
