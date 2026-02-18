using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for UpdateProfileDto validation.
/// Ensures user profile updates have valid first and last names.
/// </summary>
public class UpdateProfileDtoTests : DtoTestBase
{
    private static UpdateProfileDto CreateValidDto() => new()
    {
        FirstName = "John",
        LastName = "Doe"
    };

    // ==================== VALID DTO ====================

    [Fact]
    public void Should_Pass_ValidDto()
    {
        var dto = CreateValidDto();

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithHyphenatedAndApostropheNames()
    {
        var dto = new UpdateProfileDto
        {
            FirstName = "Mary-Jane",
            LastName = "O'Brien"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithUnicodeNames()
    {
        var dto = new UpdateProfileDto
        {
            FirstName = "José",
            LastName = "García"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithAccentedCharacters()
    {
        var dto = new UpdateProfileDto
        {
            FirstName = "François",
            LastName = "Müller"
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== FIRST NAME VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyFirstName(string? invalidFirstName)
    {
        var dto = CreateValidDto();
        dto.FirstName = invalidFirstName!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(UpdateProfileDto.FirstName));
    }

    [Theory]
    [InlineData("J")] // Too short (< 2 chars)
    [InlineData("John123")] // Has numbers
    [InlineData("John@Doe")] // Has invalid special chars
    [InlineData("!@#$")]
    [InlineData("John<script>")] // XSS attempt
    public void Should_Fail_InvalidFirstName(string invalidFirstName)
    {
        var dto = CreateValidDto();
        dto.FirstName = invalidFirstName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(UpdateProfileDto.FirstName));
    }

    [Fact]
    public void Should_Fail_FirstNameTooLong()
    {
        var dto = CreateValidDto();
        dto.FirstName = new string('A', 101); // Over 100 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(UpdateProfileDto.FirstName));
    }

    [Fact]
    public void Should_Pass_FirstNameExactlyMinLength()
    {
        var dto = CreateValidDto();
        dto.FirstName = "Jo"; // Exactly 2 chars

        var results = Validate(dto);

        AssertValidProperty(results, nameof(UpdateProfileDto.FirstName));
    }

    [Fact]
    public void Should_Pass_FirstNameExactlyMaxLength()
    {
        var dto = CreateValidDto();
        dto.FirstName = new string('A', 100); // Exactly 100 chars

        var results = Validate(dto);

        AssertValidProperty(results, nameof(UpdateProfileDto.FirstName));
    }

    // ==================== LAST NAME VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyLastName(string? invalidLastName)
    {
        var dto = CreateValidDto();
        dto.LastName = invalidLastName!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(UpdateProfileDto.LastName));
    }

    [Theory]
    [InlineData("D")] // Too short
    [InlineData("Doe123")] // Has numbers
    [InlineData("Doe<script>")] // XSS attempt
    [InlineData("Smith&Jones")] // Invalid special char
    [InlineData("Test@Email")] // Invalid @ symbol
    public void Should_Fail_InvalidLastName(string invalidLastName)
    {
        var dto = CreateValidDto();
        dto.LastName = invalidLastName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(UpdateProfileDto.LastName));
    }

    [Fact]
    public void Should_Fail_LastNameTooLong()
    {
        var dto = CreateValidDto();
        dto.LastName = new string('B', 101); // Over 100 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(UpdateProfileDto.LastName));
    }

    [Fact]
    public void Should_Pass_LastNameExactlyMinLength()
    {
        var dto = CreateValidDto();
        dto.LastName = "Wu"; // Exactly 2 chars

        var results = Validate(dto);

        AssertValidProperty(results, nameof(UpdateProfileDto.LastName));
    }

    [Fact]
    public void Should_Pass_LastNameExactlyMaxLength()
    {
        var dto = CreateValidDto();
        dto.LastName = new string('B', 100); // Exactly 100 chars

        var results = Validate(dto);

        AssertValidProperty(results, nameof(UpdateProfileDto.LastName));
    }

    // ==================== MULTIPLE ERRORS ====================

    [Fact]
    public void Should_Fail_BothFieldsEmpty()
    {
        var dto = new UpdateProfileDto
        {
            FirstName = "",
            LastName = ""
        };

        var results = Validate(dto);

        AssertInvalid(results);
        AssertErrorCount(results, 2);
    }

    [Fact]
    public void Should_Fail_BothFieldsInvalid()
    {
        var dto = new UpdateProfileDto
        {
            FirstName = "123", // Numbers
            LastName = "!@#" // Special chars
        };

        var results = Validate(dto);

        AssertInvalid(results);
        Assert.True(results.Count >= 2, "Should have at least 2 validation errors");
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_WithSpacesInNames()
    {
        var dto = new UpdateProfileDto
        {
            FirstName = "Mary Jane", // Space in first name
            LastName = "Van Der Berg" // Spaces in last name
        };

        var results = Validate(dto);

        // PersonName validator should allow spaces in names
        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithNonLatinAlphabets()
    {
        var dto = new UpdateProfileDto
        {
            FirstName = "Владимир", // Russian
            LastName = "李明" // Chinese (2+ chars required)
        };

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_WithMixedCaseNames()
    {
        var dto = new UpdateProfileDto
        {
            FirstName = "JoHn",
            LastName = "McDoNaLd"
        };

        var results = Validate(dto);

        AssertValid(results);
    }
}