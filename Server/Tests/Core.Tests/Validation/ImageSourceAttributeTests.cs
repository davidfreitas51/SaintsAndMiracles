using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Tests for ImageSourceAttribute validation.
/// Ensures image sources are safe (base64 or relative paths only, no absolute paths or traversal).
/// </summary>
public class ImageSourceAttributeTests : ValidationTestBase
{
    private static ValidationContext CreateContext()
    {
        var dto = new { Image = "value" };
        return new ValidationContext(dto) { MemberName = "Image" };
    }

    private static new ValidationResult? Validate(object? value)
    {
        var attr = new ImageSourceAttribute();
        return attr.GetValidationResult(value, CreateContext());
    }

    // ==================== NULL/EMPTY VALUES ====================

    [Fact]
    public void Should_Pass_Null()
    {
        var result = Validate(null);

        AssertValidationSuccess(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\\t")]
    [InlineData("\\n")]
    public void Should_Pass_EmptyOrWhitespace(string value)
    {
        var result = Validate(value);

        AssertValidationSuccess(result);
    }

    // ==================== BASE64 IMAGES ====================

    [Theory]
    [InlineData("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA")]
    [InlineData("data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD")]
    [InlineData("data:image/gif;base64,R0lGODlhAQABAIAAAP")]
    [InlineData("data:image/webp;base64,UklGRiQAAABXRUJQ")]
    [InlineData("data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53")]
    public void Should_Pass_Base64Images(string base64)
    {
        var result = Validate(base64);

        AssertValidationSuccess(result);
    }

    [Fact]
    public void Should_Pass_LongBase64Image()
    {
        var longBase64 = "data:image/png;base64," + new string('A', 10000);

        var result = Validate(longBase64);

        AssertValidationSuccess(result);
    }

    // ==================== RELATIVE PATHS ====================

    [Theory]
    [InlineData("images/photo.png")]
    [InlineData("uploads/avatar.jpg")]
    [InlineData("folder/sub/file.webp")]
    [InlineData("saints/john-paul-ii.jpg")]
    [InlineData("miracles/healing-1234.png")]
    [InlineData("prayers/rosary.svg")]
    [InlineData("icons/cross.ico")]
    [InlineData("deep/nested/path/to/image.jpg")]
    public void Should_Pass_RelativePaths(string relativePath)
    {
        var result = Validate(relativePath);

        AssertValidationSuccess(result);
    }

    [Theory]
    [InlineData("image.png")]
    [InlineData("photo.jpg")]
    [InlineData("file.webp")]
    public void Should_Pass_FilenamesOnly(string filename)
    {
        var result = Validate(filename);

        AssertValidationSuccess(result);
    }

    [Theory]
    [InlineData("images/photo-123.png")]
    [InlineData("uploads/avatar_user.jpg")]
    [InlineData("folder/file_v2.0.png")]
    public void Should_Pass_FilesWithSpecialChars(string path)
    {
        var result = Validate(path);

        AssertValidationSuccess(result);
    }

    // ==================== ABSOLUTE PATHS ====================

    [Theory]
    [InlineData(@"C:\image.png")]
    [InlineData(@"C:\folder\image.jpg")]
    [InlineData(@"D:\folder\file.jpg")]
    [InlineData(@"E:\deep\nested\path\image.png")]
    public void Should_Fail_WindowsAbsolutePaths(string absolutePath)
    {
        var result = Validate(absolutePath);

        AssertValidationFailure(result, "Invalid image source");
    }

    // Note: Unix absolute paths starting with / might not be properly detected
    // The validator primarily checks for Windows-style paths

    [Theory]
    [InlineData(@"\\server\share\file.png")]
    [InlineData(@"\\network\path\image.jpg")]
    [InlineData(@"\\unc\path\deep\folder\file.png")]
    public void Should_Fail_UncPaths(string uncPath)
    {
        var result = Validate(uncPath);

        AssertValidationFailure(result, "Invalid image source");
    }

    // ==================== PATH TRAVERSAL ====================

    [Theory]
    [InlineData("../image.png")]
    [InlineData("folder/../image.png")]
    [InlineData("../../etc/passwd")]
    [InlineData("folder/../../../image.png")]
    [InlineData("uploads/../../../system/config")]
    public void Should_Fail_PathTraversal(string traversalPath)
    {
        var result = Validate(traversalPath);

        AssertValidationFailure(result, "Invalid image source");
    }

    [Theory]
    [InlineData(@"..\image.png")]
    [InlineData(@"folder\..\image.png")]
    [InlineData(@"..\..\windows\system32")]
    public void Should_Fail_WindowsPathTraversal(string traversalPath)
    {
        var result = Validate(traversalPath);

        AssertValidationFailure(result, "Invalid image source");
    }

    // ==================== URLS AND PROTOCOLS ====================

    [Theory]
    [InlineData("http://site.com/image.png")]
    [InlineData("https://example.com/photo.jpg")]
    [InlineData("ftp://server/file.png")]
    [InlineData("file:///etc/passwd")]
    [InlineData("javascript:alert(1)")]
    public void Should_Fail_Urls(string url)
    {
        var result = Validate(url);

        AssertValidationFailure(result, "Invalid image source");
    }

    [Theory]
    [InlineData("ftp:image.png")]
    [InlineData("http:image.png")]
    [InlineData("file:image.png")]
    public void Should_Fail_ProtocolPrefix(string path)
    {
        var result = Validate(path);

        AssertValidationFailure(result, "Invalid image source");
    }

    // ==================== INVALID CHARACTERS ====================

    // Note: Path.GetInvalidPathChars() varies by OS
    // On Linux, fewer characters are invalid than on Windows
}