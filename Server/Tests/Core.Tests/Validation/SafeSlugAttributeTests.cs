using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Tests for SafeSlugAttribute validation.
/// Ensures URL slugs are properly formatted and safe for use in URLs.
/// </summary>
public class SafeSlugAttributeTests : ValidationTestBase
{
    private class TestModel
    {
        [SafeSlug]
        public string? Slug { get; set; }
    }

    // ==================== VALID SLUGS ====================

    [Theory]
    [InlineData("simple")]
    [InlineData("with-dash")]
    [InlineData("multiple-dashes-here")]
    [InlineData("saint-john-paul-ii")]
    [InlineData("numbers-123")]
    [InlineData("saint-999")]
    [InlineData("a")]
    public void Should_Pass_ValidSlugs(string validSlug)
    {
        var model = new TestModel { Slug = validSlug };
        
        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_VeryLongSlug()
    {
        var model = new TestModel { Slug = new string('a', 150) };
        
        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_Null()
    {
        var model = new TestModel { Slug = null };
        
        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== EMPTY/WHITESPACE ====================

    [Fact]
    public void Should_Fail_Empty()
    {
        var model = new TestModel { Slug = "" };
        
        var results = Validate(model);

        // Empty string triggers "cannot be empty" check first
        AssertInvalid(results, nameof(TestModel.Slug), "cannot be empty");
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Should_Fail_Whitespace(string whitespaceSlug)
    {
        var model = new TestModel { Slug = whitespaceSlug };
        
        var results = Validate(model);

        // Whitespace triggers "cannot be empty" check after trimming
        AssertInvalid(results, nameof(TestModel.Slug), "cannot be empty");
    }

    // ==================== DASH VALIDATION ====================

    [Theory]
    [InlineData("-invalid-slug")]
    [InlineData("-")]
    [InlineData("-start")]
    public void Should_Fail_StartsWithDash(string invalidSlug)
    {
        var model = new TestModel { Slug = invalidSlug };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Slug), "invalid format");
    }

    [Theory]
    [InlineData("invalid-slug-")]
    [InlineData("end-")]
    public void Should_Fail_EndsWithDash(string invalidSlug)
    {
        var model = new TestModel { Slug = invalidSlug };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Slug), "invalid format");
    }

    [Theory]
    [InlineData("invalid--slug")]
    [InlineData("double--dash")]
    [InlineData("triple---dash")]
    [InlineData("a--b--c")]
    public void Should_Fail_ConsecutiveDashes(string invalidSlug)
    {
        var model = new TestModel { Slug = invalidSlug };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Slug), "invalid format");
    }

    // ==================== CASE SENSITIVITY ====================

    [Theory]
    [InlineData("Invalid-slug")]
    [InlineData("ALLCAPS")]
    [InlineData("MixedCase")]
    [InlineData("camelCase")]
    [InlineData("PascalCase")]
    [InlineData("Slug-With-Caps")]
    [InlineData("slug-With-One-Cap")]
    public void Should_Fail_Uppercase(string invalidSlug)
    {
        var model = new TestModel { Slug = invalidSlug };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Slug), "invalid format");
    }

    // ==================== SPECIAL CHARACTERS ====================

    [Theory]
    [InlineData("slug@123")]
    [InlineData("slug#tag")]
    [InlineData("slug$money")]
    [InlineData("slug%percent")]
    [InlineData("slug&and")]
    [InlineData("slug*star")]
    [InlineData("slug+plus")]
    [InlineData("slug=equals")]
    [InlineData("slug[brackets]")]
    [InlineData("slug{braces}")]
    [InlineData("slug|pipe")]
    [InlineData("slug\\backslash")]
    [InlineData("slug/slash")]
    [InlineData("slug:colon")]
    [InlineData("slug;semicolon")]
    [InlineData("slug'quote")]
    [InlineData("slug\\\"doublequote")]
    [InlineData("slug<less")]
    [InlineData("slug>greater")]
    [InlineData("slug?question")]
    [InlineData("slug!exclamation")]
    [InlineData("slug.period")]
    [InlineData("slug,comma")]
    public void Should_Fail_SpecialCharacters(string invalidSlug)
    {
        var model = new TestModel { Slug = invalidSlug };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Slug), "invalid format");
    }

    [Theory]
    [InlineData("invalid slug")]
    [InlineData("slug with spaces")]
    [InlineData("slug  double")]
    [InlineData(" leadingspace")]
    [InlineData("trailingspace ")]
    public void Should_Fail_Spaces(string invalidSlug)
    {
        var model = new TestModel { Slug = invalidSlug };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Slug), "invalid format");
    }

    [Theory]
    [InlineData("slug\\ttab")]
    [InlineData("slug\\nnewline")]
    [InlineData("slug\\rreturn")]
    [InlineData("slug\\u0001control")]
    public void Should_Fail_ControlCharacters(string invalidSlug)
    {
        var model = new TestModel { Slug = invalidSlug };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Slug), "invalid format");
    }

    // ==================== LENGTH VALIDATION ====================

    [Fact]
    public void Should_Fail_TooLong()
    {
        var model = new TestModel { Slug = new string('a', 151) };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Slug), "invalid format");
    }

    [Fact]
    public void Should_Pass_ExactlyMaxLength()
    {
        var model = new TestModel { Slug = new string('a', 150) };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_SingleCharacter()
    {
        var model = new TestModel { Slug = "a" };

        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== UNICODE ====================

    [Theory]
    [InlineData("slug-with-日本語")]
    [InlineData("العربية-slug")]
    [InlineData("slug-ελληνικά")]
    [InlineData("slugári")]
    public void Should_Fail_UnicodeCharacters(string invalidSlug)
    {
        var model = new TestModel { Slug = invalidSlug };
        
        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Slug), "invalid format");
    }
}