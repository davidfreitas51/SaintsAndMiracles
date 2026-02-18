using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for InviteRequest validation.
/// Ensures invitation roles are safe and properly formatted.
/// </summary>
public class InviteRequestTests : DtoTestBase
{
    private static InviteRequest CreateValidDto() => new()
    {
        Role = "Editor"
    };

    // ==================== VALID DTO ====================

    [Fact]
    public void Should_Pass_DefaultValue()
    {
        var dto = new InviteRequest();

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_ValidDto()
    {
        var dto = CreateValidDto();

        var results = Validate(dto);

        AssertValid(results);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Editor")]
    [InlineData("Contributor")]
    [InlineData("Admin-User")]
    public void Should_Pass_ValidRoles(string validRole)
    {
        var dto = CreateValidDto();
        dto.Role = validRole;

        var results = Validate(dto);

        AssertValid(results);
    }

    // ==================== ROLE VALIDATION ====================

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Fail_EmptyRole(string emptyRole)
    {
        var dto = CreateValidDto();
        dto.Role = emptyRole;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(InviteRequest.Role));
    }

    [Theory]
    [InlineData("<script>alert(1)</script>")]
    [InlineData("<b>Admin</b>")]
    [InlineData("Role&amp;Admin")]
    public void Should_Fail_UnsafeRole(string unsafeRole)
    {
        var dto = CreateValidDto();
        dto.Role = unsafeRole;

        var results = Validate(dto);

        AssertInvalidProperty(results, nameof(InviteRequest.Role));
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Pass_RoleWithNumbers()
    {
        var dto = CreateValidDto();
        dto.Role = "Admin2024";

        var results = Validate(dto);

        AssertValid(results);
    }

    [Fact]
    public void Should_Pass_RoleWithUnderscore()
    {
        var dto = CreateValidDto();
        dto.Role = "Super_Admin";

        var results = Validate(dto);

        AssertValid(results);
    }
}
