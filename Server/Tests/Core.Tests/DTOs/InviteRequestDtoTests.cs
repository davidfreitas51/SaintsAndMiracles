using Xunit;

namespace Core.Tests.DTOs;

public class InviteRequestTests
{
    [Fact]
    public void Should_Be_Valid_When_Default_Value_Is_Used()
    {
        var dto = new InviteRequest();

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Be_Valid_When_Role_Is_SafeText()
    {
        var dto = new InviteRequest
        {
            Role = "Editor"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    [Fact]
    public void Should_Fail_When_Role_Is_Empty()
    {
        var dto = new InviteRequest
        {
            Role = ""
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(InviteRequest.Role))
        );
    }

    [Fact]
    public void Should_Fail_When_Role_Is_Unsafe()
    {
        var dto = new InviteRequest
        {
            Role = "<script>alert(1)</script>"
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(InviteRequest.Role))
        );
    }
}
