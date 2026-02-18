using System.ComponentModel.DataAnnotations;
using Core.Validation.Attributes;
using Xunit;

namespace Core.Tests.Validation;

/// <summary>
/// Tests for SafeTokenAttribute validation.
/// Ensures tokens contain only safe characters (typically alphanumeric).
/// </summary>
public class SafeTokenAttributeTests : ValidationTestBase
{
    private class TestModel
    {
        [SafeToken]
        public string? Token { get; set; }
    }

    // ==================== NULL VALUES ====================

    [Fact]
    public void Should_Pass_Null()
    {
        var model = new TestModel { Token = null };

        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== EMPTY/WHITESPACE ====================

    [Fact]
    public void Should_Fail_Empty()
    {
        var model = new TestModel { Token = "" };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Token), "invalid format");
    }

    [Theory]
    [InlineData("   ")]
    [InlineData("\\t")]
    [InlineData("\\n")]
    [InlineData("\\r\\n")]
    public void Should_Fail_Whitespace(string whitespace)
    {
        var model = new TestModel { Token = whitespace };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Token), "invalid format");
    }

    // ==================== VALID TOKENS ====================

    [Theory]
    [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ012345")] // 32 chars
    [InlineData("abcdefghijklmnopqrstuvwxyz012345")] // 32 chars
    [InlineData("ABC123def456GHI789jkl012MNO34567")] // 32 chars
    [InlineData("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")] // 32 A's
    public void Should_Pass_ValidTokens(string validToken)
    {
        var model = new TestModel { Token = validToken };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_ExactlyMinLength()
    {
        var model = new TestModel { Token = new string('A', 32) };

        var results = Validate(model);

        AssertValid(results);
    }

    // ==================== INVALID CHARACTERS ====================

    [Theory]
    [InlineData("Invalid$TokenWith32CharsLongHere!!")]
    [InlineData("TokenWith@SignAnd32CharsExactlyAAA")]
    [InlineData("Token#Hash")]
    [InlineData("Token%Percent")]
    [InlineData("Token&Ampersand")]
    [InlineData("Token*Asterisk")]
    [InlineData("Token+Plus")]
    [InlineData("Token=Equals")]
    [InlineData("Token-Dash")]
    [InlineData("Token_Underscore")]
    [InlineData("Token.Dot")]
    [InlineData("Token,Comma")]
    [InlineData("Token;Semicolon")]
    [InlineData("Token:Colon")]
    [InlineData("Token!Exclamation")]
    [InlineData("Token?Question")]
    [InlineData("Token/Slash")]
    [InlineData("Token\\\\Backslash")]
    [InlineData("Token|Pipe")]
    [InlineData("Token<Less")]
    [InlineData("Token>Greater")]
    [InlineData("Token[Bracket")]
    [InlineData("Token]Bracket")]
    [InlineData("Token{Brace")]
    [InlineData("Token}Brace")]
    [InlineData("Token(Paren")]
    [InlineData("Token)Paren")]
    [InlineData("Token'Quote")]
    [InlineData("Token\\\"DoubleQuote")]
    public void Should_Fail_SpecialCharacters(string invalidToken)
    {
        var model = new TestModel { Token = invalidToken };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Token), "invalid format");
    }

    [Theory]
    [InlineData("TokenWith SpaceInsideAndValidLength1234")]
    [InlineData("Token Has Spaces")]
    [InlineData("  LeadingSpaces")]
    [InlineData("TrailingSpaces  ")]
    public void Should_Fail_Spaces(string invalidToken)
    {
        var model = new TestModel { Token = invalidToken };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Token), "invalid format");
    }

    [Theory]
    [InlineData("TokenWith\tTabCharacterAndValidLengthAB")]
    [InlineData("TokenWith\\nNewlineCharacterAndValidLen")]
    [InlineData("TokenWith\\rReturnCharacterAndValidLen")]
    [InlineData("Token\\u0001ControlChar")]
    [InlineData("Token\\u001FUnitSeparator")]
    public void Should_Fail_ControlCharacters(string invalidToken)
    {
        var model = new TestModel { Token = invalidToken };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Token), "invalid format");
    }

    // ==================== LENGTH ====================

    [Fact]
    public void Should_Fail_TooShort()
    {
        var model = new TestModel { Token = "abc" };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Token), "invalid format");
    }

    [Fact]
    public void Should_Pass_LongToken()
    {
        var model = new TestModel { Token = new string('A', 100) };

        var results = Validate(model);

        AssertValid(results);
    }

    [Fact]
    public void Should_Fail_TooLong()
    {
        var model = new TestModel { Token = new string('A', 129) };

        var results = Validate(model);

        AssertInvalid(results, nameof(TestModel.Token), "invalid format");
    }

    // ==================== TYPE VALIDATION ====================

    [Fact]
    public void Should_Fail_NonStringType()
    {
        var attr = new SafeTokenAttribute();
        var context = new ValidationContext(new object()) { MemberName = "Token" };

        var result = attr.GetValidationResult(123, context);

        AssertValidationFailure(result, "invalid format");
    }
}
