using Xunit;

namespace Core.Tests.DTOs;

public class NewMiracleDtoTests
{
    private static NewMiracleDto CreateValidDto() => new()
    {
        Title = "Milagre de Lourdes",
        Country = "França",
        Century = 19,
        Image = "images/lourdes.jpg",
        Description = "Descrição do milagre",
        MarkdownContent = "milagre-lourdes.md",
        Date = "1858",
        LocationDetails = "Lourdes, França",
        TagIds = [1, 2, 3]
    };

    [Fact]
    public void Should_Fail_When_Required_Fields_Are_Missing()
    {
        var dto = new NewMiracleDto
        {
            Title = "",
            Country = "",
            Image = "",
            Description = "",
            MarkdownContent = ""
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.True(results.Count >= 5);
    }

    [Fact]
    public void Should_Fail_When_Title_Is_Unsafe()
    {
        var dto = CreateValidDto();
        dto.Title = "<script>alert(1)</script>";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewMiracleDto.Title))
        );
    }

    [Fact]
    public void Should_Fail_When_Description_Is_Unsafe()
    {
        var dto = CreateValidDto();
        dto.Description = "<b>HTML</b>";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewMiracleDto.Description))
        );
    }

    [Fact]
    public void Should_Fail_When_TagIds_Exceed_Max_Limit()
    {
        var dto = CreateValidDto();
        dto.TagIds = [1, 2, 3, 4, 5, 6];

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewMiracleDto.TagIds))
        );
    }

    [Fact]
    public void Should_Fail_When_Optional_Date_Is_Unsafe()
    {
        var dto = CreateValidDto();
        dto.Date = "<script>1858</script>";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewMiracleDto.Date))
        );
    }

    [Fact]
    public void Should_Fail_When_Optional_LocationDetails_Is_Unsafe()
    {
        var dto = CreateValidDto();
        dto.LocationDetails = "<img src=x onerror=alert(1)>";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewMiracleDto.LocationDetails))
        );
    }

    [Fact]
    public void Should_Pass_When_Dto_Is_Valid()
    {
        var dto = CreateValidDto();

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }
}
