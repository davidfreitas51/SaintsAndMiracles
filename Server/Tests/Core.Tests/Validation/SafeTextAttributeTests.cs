using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Tests for SafeTextAttribute validation.
/// Ensures text content is safe from HTML injection and XSS attacks.
/// </summary>
public class SafeTextAttributeTests : ValidationTestBase
{
    private class TestModel
    {
        [SafeText]
        public string? Text { get; set; }

        [SafeText(ErrorMessage = "Custom error message")]
        public string? CustomText { get; set; }

        [SafeText]
        public object? NotAString { get; set; }
    }

    // ==================== VALID TEXT ====================

    [Fact]
    public void Should_Pass_Null()
    {
        var model = new TestModel { Text = null };

        var results = Validate(model);

        AssertValid(results);
    }

    [Theory]
    [InlineData("Just a normal text")]
    [InlineData("Text with 123 numbers")]
    [InlineData("Text with punctuation! How are you?")]
    [InlineData("Multiple\nLines\nOf\nText")]
    [InlineData("Unicode characters: 日本語, العربية, Ελληνικά")]
    [InlineData("Email-like text: contact at example.com")]
    [InlineData("URL mentions: example.com (no protocol)")]
    public void Should_Pass_SafeText(string safeText)
    {
        var model = new TestModel { Text = safeText };

        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== HTML TAGS ====================

    [Theory]
    [InlineData("<b>bold</b>")]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src=x>")]
    [InlineData("<div>content</div>")]
    [InlineData("Text before <span>tag</span> text after")]
    [InlineData("<a href='malicious'>click</a>")]
    [InlineData("</br>")]
    [InlineData("<style>body{display:none}</style>")]
    [InlineData("<!-- comment -->")]
    public void Should_Fail_HtmlTags(string textWithHtml)
    {
        var model = new TestModel { Text = textWithHtml };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Text), "invalid format");
    }

    // ==================== HTML ENTITIES ====================

    [Theory]
    [InlineData("Hello &amp; World")]
    [InlineData("Copyright &copy; 2024")]
    [InlineData("Less than &lt; and greater than &gt;")]
    [InlineData("Non-breaking&nbsp;space")]
    [InlineData("Quote&quot;marks")]
    [InlineData("&apos;apostrophe")]
    public void Should_Fail_HtmlEntities(string textWithEntities)
    {
        var model = new TestModel { Text = textWithEntities };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Text), "invalid format");
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_EmptyString()
    {
        var model = new TestModel { Text = "" };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WhitespaceOnly()
    {
        var model = new TestModel { Text = "   \t\n  " };

        var results = Validate(model);

        AssertValid(results);
    }

    [Theory]
    [InlineData("Text with < but no closing")]
    [InlineData("Text with > but no opening")]
    public void Should_Pass_IsolatedBrackets(string textWithBrackets)
    {
        var model = new TestModel { Text = textWithBrackets };

        var results = Validate(model);

        AssertValid(results);
    }

    // Note: "Math: 5 < 10 > 3" looks like a tag "< 10 >" to the HTML regex
    [Fact]
    public void Should_Fail_MathExpressionLookingLikeTag()
    {
        var model = new TestModel { Text = "Math: 5 < 10 > 3" };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Text), "invalid format");
    }

    // ==================== CUSTOM ERROR MESSAGE ====================

    [Fact]
    public void Should_UseCustomErrorMessage()
    {
        var model = new TestModel { CustomText = "<i>test</i>" };

        var results = Validate(model);

        Assert.Single(results);
        Assert.Contains(nameof(TestModel.CustomText), results[0].MemberNames);
        Assert.Equal("Custom error message", results[0].ErrorMessage);
    }

    // ==================== TYPE VALIDATION ====================

    [Fact]
    public void Should_Fail_NonStringType()
    {
        var model = new TestModel { NotAString = 123 };

        var results = Validate(model);

        AssertInvalid(results, null, "SafeText can only be applied to string fields");
    }
}
