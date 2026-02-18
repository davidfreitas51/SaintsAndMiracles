using Core.DTOs;
using Core.Enums;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for NewTagDto validation.
/// Ensures tag names are safe and of valid length with proper type assignment.
/// </summary>
public class NewTagDtoTests : DtoTestBase
{
    private static NewTagDto CreateValidDto() => new()
    {
        Name = "Valid Tag",
        TagType = TagType.Saint
    };

    // ==================== VALID DTO ====================

    [Fact]
    public void Should_Pass_ValidDto()
    {
        var dto = CreateValidDto();

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData(TagType.Saint)]
    [InlineData(TagType.Miracle)]
    [InlineData(TagType.Prayer)]
    public void Should_Pass_AllTagTypes(TagType tagType)
    {
        var dto = CreateValidDto();
        dto.TagType = tagType;

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== NAME VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Should_Fail_EmptyName(string? invalidName)
    {
        var dto = CreateValidDto();
        dto.Name = invalidName!;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewTagDto.Name));
    }

    [Theory]
    [InlineData("AB")] // Too short (< 3 chars)
    [InlineData("A")]
    public void Should_Fail_NameTooShort(string shortName)
    {
        var dto = CreateValidDto();
        dto.Name = shortName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewTagDto.Name));
    }

    [Theory]
    [InlineData("<b>Bold</b>")]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("Tag&amp;Name")]
    public void Should_Fail_UnsafeName(string unsafeName)
    {
        var dto = CreateValidDto();
        dto.Name = unsafeName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewTagDto.Name));
    }

    [Fact]
    public void Should_Fail_NameTooLong()
    {
        var dto = CreateValidDto();
        dto.Name = new string('A', 101); // Over 100 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewTagDto.Name));
    }

    [Fact]
    public void Should_Pass_NameExactlyMaxLength()
    {
        var dto = CreateValidDto();
        dto.Name = new string('A', 100); // Exactly 100 chars

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewTagDto.Name));
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_NameWithHyphen()
    {
        var dto = CreateValidDto();
        dto.Name = "Saint-Related";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_NameWithUnicode()
    {
        var dto = CreateValidDto();
        dto.Name = "São Paulo";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_NameWithNumbers()
    {
        var dto = CreateValidDto();
        dto.Name = "Tag 123";

        var results = Validate(dto);

        AssertValid(results);
    }
}
