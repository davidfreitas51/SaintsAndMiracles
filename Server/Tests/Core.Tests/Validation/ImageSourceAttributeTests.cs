using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

public class ImageSourceAttributeTests
{
    private static ValidationContext CreateContext()
    {
        var dto = new { Image = "value" };

        return new ValidationContext(dto)
        {
            MemberName = "Image"
        };
    }

    private static ValidationResult? Validate(object? value)
    {
        var attr = new ImageSourceAttribute();

        return attr.GetValidationResult(value, CreateContext());
    }

    // =====================
    // VALID CASES
    // =====================

    [Fact]
    public void IsValid_Should_Accept_Null()
    {
        var result = Validate(null);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_Should_Accept_Empty_Or_Whitespace(string value)
    {
        var result = Validate(value);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Fact]
    public void IsValid_Should_Accept_Base64_Image()
    {
        var value = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA";

        var result = Validate(value);

        Assert.Equal(ValidationResult.Success, result);
    }

    [Theory]
    [InlineData("images/photo.png")]
    [InlineData("uploads/avatar.jpg")]
    [InlineData("folder/sub/file.webp")]
    public void IsValid_Should_Accept_Relative_Paths(string value)
    {
        var result = Validate(value);

        Assert.Equal(ValidationResult.Success, result);
    }

    // =====================
    // INVALID CASES
    // =====================

    [Theory]
    [InlineData(@"C:\image.png")]
    [InlineData(@"D:\folder\file.jpg")]
    public void IsValid_Should_Reject_Windows_Absolute_Paths(string value)
    {
        var result = Validate(value);

        Assert.NotNull(result);
        Assert.Equal("Invalid image source.", result!.ErrorMessage);
    }

    [Fact]
    public void IsValid_Should_Reject_Unc_Paths()
    {
        var result = Validate(@"\\server\share\file.png");

        Assert.NotNull(result);
        Assert.Equal("Invalid image source.", result!.ErrorMessage);
    }

    [Theory]
    [InlineData("../image.png")]
    [InlineData("folder/../image.png")]
    public void IsValid_Should_Reject_PathTraversal(string value)
    {
        var result = Validate(value);

        Assert.NotNull(result);
        Assert.Equal("Invalid image source.", result!.ErrorMessage);
    }

    [Theory]
    [InlineData("http://site.com/image.png")]
    [InlineData("ftp:image.png")]
    public void IsValid_Should_Reject_Colon(string value)
    {
        var result = Validate(value);

        Assert.NotNull(result);
        Assert.Equal("Invalid image source.", result!.ErrorMessage);
    }

    [Fact]
    public void IsValid_Should_Reject_Invalid_Path_Chars()
    {
        var invalid = $"image{Path.GetInvalidPathChars()[0]}file.png";

        var result = Validate(invalid);

        Assert.NotNull(result);
        Assert.Equal("Invalid image source.", result!.ErrorMessage);
    }
}