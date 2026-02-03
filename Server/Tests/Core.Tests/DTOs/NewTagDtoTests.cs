using Core.DTOs;
using Core.Enums;
using Xunit;

namespace Core.Tests.DTOs;

public class NewTagDtoTests
{
    private static NewTagDto CreateValidDto() => new()
    {
        Name = "Valid Tag Name",
        TagType = TagType.Saint
    };

    [Fact]
    public void Should_Pass_When_Dto_Is_Valid()
    {
        var dto = CreateValidDto();

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Null()
    {
        var dto = CreateValidDto();
        dto.Name = null!;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewTagDto.Name)));
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var dto = CreateValidDto();
        dto.Name = string.Empty;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewTagDto.Name)));
    }

    [Fact]
    public void Should_Fail_When_Name_Contains_HtmlTag()
    {
        var dto = CreateValidDto();
        dto.Name = "<b>Bold</b>";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewTagDto.Name)));
    }

    [Fact]
    public void Should_Fail_When_Name_Contains_HtmlEntity()
    {
        var dto = CreateValidDto();
        dto.Name = "Tom &amp; Jerry";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(NewTagDto.Name)));
    }
}
