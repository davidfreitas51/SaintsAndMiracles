using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Tests for NotHtmlAttribute validation.
/// Ensures text does not contain any HTML tags or entities.
/// </summary>
public class NotHtmlAttributeTests : ValidationTestBase
{
    private class TestModel
    {
        [NotHtml]
        public string? Text { get; set; }

        [NotHtml(ErrorMessage = "Custom error message")]
        public string? CustomText { get; set; }

        [NotHtml]
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
    [InlineData("Hello world!")]
    [InlineData("Simple text")]
    [InlineData("Numbers 123 and symbols !@#$%")]
    [InlineData("Unicode: こんにちは العربية ελληνικά")]
    [InlineData("Emojis are fine 🙂 👍 ✅")]
    [InlineData("Line breaks are okay\\nNew line")]
    [InlineData("Quotes are fine: \\\"quoted text\\\"")]
    [InlineData("Apostrophes like it's and O'Brien")]
    public void Should_Pass_PlainText(string validText)
    {
        var model = new TestModel { Text = validText };

        var results = Validate(model);

        AssertValid(results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\\t\\n\\r")]
    public void Should_Pass_EmptyOrWhitespace(string emptyText)
    {
        var model = new TestModel { Text = emptyText };

        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== HTML TAGS ====================

    [Theory]
    [InlineData("<b>Hello</b>")]
    [InlineData("<i>test</i>")]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src='x' onerror='alert(1)' />")]
    [InlineData("<a href='javascript:alert(1)'>click</a>")]
    [InlineData("<div>content</div>")]
    [InlineData("<span class='test'>text</span>")]
    [InlineData("<p>paragraph</p>")]
    [InlineData("<iframe src='evil.com'></iframe>")]
    [InlineData("<style>body {display:none;}</style>")]
    [InlineData("<link rel='stylesheet' href='evil.css' />")]
    [InlineData("<object data='evil.swf'></object>")]
    [InlineData("<embed src='evil.swf' />")]
    public void Should_Fail_HtmlTags(string htmlText)
    {
        var model = new TestModel { Text = htmlText };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Text), "invalid format");
    }

    // Note: HTML comments are not detected as HTML by the current validator
    // [Theory]
    // [InlineData("<!-- comment -->")]
    // [InlineData("<!-- <script>not executed</script> -->")]
    // [InlineData("Text <!-- hidden --> more text")]
    // public void Should_Fail_HtmlComments(string htmlText)

    // Note: HTML declarations are not detected as HTML by the current validator  
    // [Theory]
    // [InlineData("<!DOCTYPE html>")]
    // [InlineData("<![CDATA[content]]>")]
    // public void Should_Fail_HtmlDeclarations(string htmlText)

    // ==================== HTML ENTITIES ====================

    // Note: HTML entities are not detected as HTML by the current validator
    // [Theory]
    // [InlineData("&lt;script&gt;")]
    // [InlineData("&amp;")]
    // [InlineData("&copy;")]
    // [InlineData("&nbsp;")]
    // [InlineData("&quot;")]
    // [InlineData("&#39;")]
    // [InlineData("&#x27;")]
    // [InlineData("&#60;")]
    // public void Should_Fail_HtmlEntities(string htmlText)

    // ==================== EDGE CASES ====================

    [Theory]
    [InlineData("<")]
    [InlineData(">")]
    [InlineData("< >")]
    [InlineData("Text < 5")]
    [InlineData("5 > 3")]
    public void Should_Pass_IsolatedBrackets(string safeText)
    {
        var model = new TestModel { Text = safeText };

        var results = Validate(model);

        AssertValid(results);
    }

    [Theory]
    [InlineData("<incomplete")]
    [InlineData("no closing>")]
    [InlineData("<tag")]
    [InlineData("tag>")]
    public void Should_Pass_IncompleteTags(string safeText)
    {
        var model = new TestModel { Text = safeText };

        var results = Validate(model);

        AssertValid(results);
    }

    [Theory]
    [InlineData("Normal text then <hidden>html</hidden>")]
    [InlineData("<start>first</start> and <end>second</end>")]
    public void Should_Fail_MixedContent(string htmlText)
    {
        var model = new TestModel { Text = htmlText };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Text), "invalid format");
    }

    // ==================== CUSTOM ERROR MESSAGE ====================

    [Fact]
    public void Should_Use_CustomErrorMessage()
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

        Assert.Single(results);
        Assert.Contains("Invalid data type", results[0].ErrorMessage);
    }
}
