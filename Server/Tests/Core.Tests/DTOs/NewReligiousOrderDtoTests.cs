using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for NewReligiousOrderDto validation.
/// Ensures religious order names are safe and properly formatted.
/// </summary>
public class NewReligiousOrderDtoTests : DtoTestBase
{
    private static NewReligiousOrderDto CreateValidDto() => new()
    {
        Name = "Franciscan Order"
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
    [InlineData("Benedictine")]
    [InlineData("Dominican Order")]
    [InlineData("Jesuit Society")]
    public void Should_Pass_ValidOrderNames(string validName)
    {
        var dto = CreateValidDto();
        dto.Name = validName;

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

        AssertInvalidProperty(results, nameof(NewReligiousOrderDto.Name));
    }

    [Theory]
    [InlineData("AB")] // Too short (< 3 chars)
    [InlineData("A")]
    public void Should_Fail_NameTooShort(string shortName)
    {
        var dto = CreateValidDto();
        dto.Name = shortName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewReligiousOrderDto.Name));
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<b>Order</b>")]
    [InlineData("Order&amp;Name")]
    public void Should_Fail_UnsafeName(string unsafeName)
    {
        var dto = CreateValidDto();
        dto.Name = unsafeName;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewReligiousOrderDto.Name));
    }

    [Fact]
    public void Should_Fail_NameTooLong()
    {
        var dto = CreateValidDto();
        dto.Name = new string('A', 101); // Over 100 chars

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(NewReligiousOrderDto.Name));
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_NameExactlyMinLength()
    {
        var dto = CreateValidDto();
        dto.Name = "OSB"; // Exactly 3 chars

        var results = Validate(dto);

        AssertValidProperty(results, nameof(NewReligiousOrderDto.Name));
    }

    [Fact]
    public void Should_Pass_NameWithSpecialCharacters()
    {
        var dto = CreateValidDto();
        dto.Name = "Order of St. James";

        var results = Validate(dto);

        AssertValid(results);
    }
}
