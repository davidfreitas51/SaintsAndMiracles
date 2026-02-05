using Xunit;

namespace Core.Tests.DTOs;

public class NewSaintDtoTests
{

    private static NewSaintDto CreateValidDto() => new()
    {
        Name = "Saint Francis of Assisi",
        Country = "Italy",
        Century = 12,
        Image = "saints/francis.jpg",
        Description = "Founder of the Franciscan Order.",
        MarkdownContent = "# Saint Francis\nValid text",
        Title = "Confessor",
        FeastDay = new DateOnly(1182, 10, 4),
        PatronOf = "Animals and the environment",
        ReligiousOrderId = 1,
        TagIds = [1, 2],
        Slug = "saint-francis-of-assisi"
    };


    // ---------- VALID ----------

    [Fact]
    public void Should_Pass_When_Dto_Is_Valid()
    {
        var dto = CreateValidDto();

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    // ---------- Name ----------

    [Fact]
    public void Should_Fail_When_Name_Is_Missing()
    {
        var dto = CreateValidDto();
        dto.Name = "";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewSaintDto.Name))
        );
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Invalid_PersonName()
    {
        var dto = CreateValidDto();
        dto.Name = "123@@@";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewSaintDto.Name))
        );
    }

    // ---------- Country ----------

    [Fact]
    public void Should_Fail_When_Country_Is_Missing()
    {
        var dto = CreateValidDto();
        dto.Country = "";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewSaintDto.Country))
        );
    }

    // ---------- Century ----------

    [Theory]
    [InlineData(-51)]
    [InlineData(22)]
    public void Should_Fail_When_Century_Is_Out_Of_Range(int century)
    {
        var dto = CreateValidDto();
        dto.Century = century;

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewSaintDto.Century))
        );
    }

    // ---------- Description ----------

    [Fact]
    public void Should_Fail_When_Description_Is_Missing()
    {
        var dto = CreateValidDto();
        dto.Description = "";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewSaintDto.Description))
        );
    }

    // ---------- MarkdownContent ----------

    [Fact]
    public void Should_Fail_When_MarkdownContent_Is_Missing()
    {
        var dto = CreateValidDto();
        dto.MarkdownContent = "";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewSaintDto.MarkdownContent))
        );
    }

    // ---------- TagIds ----------

    [Fact]
    public void Should_Fail_When_TagIds_Exceed_Max_Limit()
    {
        var dto = CreateValidDto();
        dto.TagIds = [1, 2, 3, 4, 5, 6];

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewSaintDto.TagIds))
        );
    }

    // ---------- Slug ----------

    [Fact]
    public void Should_Fail_When_Slug_Is_Invalid()
    {
        var dto = CreateValidDto();
        dto.Slug = "Slug Inválido !!!";

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(NewSaintDto.Slug))
        );
    }
}
