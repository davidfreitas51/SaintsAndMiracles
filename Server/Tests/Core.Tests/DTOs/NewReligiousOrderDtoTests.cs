using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

public class NewReligiousOrderDtoTests
{
    private static NewReligiousOrderDto CreateValidDto() => new()
    {
        Name = "Franciscan Order"
    };

    [Fact]
    public void Should_Pass_When_Dto_Is_Valid()
    {
        var dto = CreateValidDto();

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Missing()
    {
        var dto = CreateValidDto();
        dto.Name = "";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewReligiousOrderDto.Name))
        );
    }

    [Fact]
    public void Should_Fail_When_Name_Contains_Html()
    {
        var dto = CreateValidDto();
        dto.Name = "<script>alert(1)</script>";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewReligiousOrderDto.Name))
        );
    }
}
