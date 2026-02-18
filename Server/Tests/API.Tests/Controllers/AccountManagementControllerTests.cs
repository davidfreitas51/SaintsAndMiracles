using API.Controllers;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Tests.Common;

namespace API.Tests.Controllers;

/// <summary>
/// Test suite for AccountManagementController.
/// Tests user profile management, password changes, and email changes.
/// </summary>
public class AccountManagementControllerTests : ControllerTestBase<AccountManagementController>
{
    private Mock<IEmailSender<AppUser>> _emailSenderMock = null!;
    private IConfiguration _configuration = null!;

    /// <summary>
    /// Helper to setup controller with mocked dependencies.
    /// </summary>
    private void SetupController()
    {
        _emailSenderMock = CreateLooseMock<IEmailSender<AppUser>>();

        var configValues = new Dictionary<string, string?>
        {
            { "Frontend:BaseUrl", "http://localhost:3000" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        SetupAuthenticatedController((userManager, signInManager) =>
            new AccountManagementController(
                userManager.Object,
                signInManager.Object,
                _emailSenderMock.Object,
                _configuration,
                GetNullLogger<AccountManagementController>()
            )
        );
    }

    // ===================== TESTS =====================

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUser_WhenAuthenticated()
    {
        SetupController();

        var user = new AppUser
        {
            Id = AuthenticatedUser.Id,
            Email = AuthenticatedUser.Email,
            FirstName = "John",
            LastName = "Doe"
        };

        UserManagerMock
            .Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        var result = await Controller.GetCurrentUser();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CurrentUserDto>(ok.Value);

        Assert.Equal("John", dto.FirstName);
        Assert.Equal("Doe", dto.LastName);
        Assert.Equal(AuthenticatedUser.Email, dto.Email);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenNoUser()
    {
        SetupController();

        UserManagerMock
            .Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync((AppUser?)null);

        var result = await Controller.GetCurrentUser();

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task UpdateProfile_ShouldUpdateNames_WhenValid()
    {
        SetupController();

        var user = new AppUser
        {
            Id = AuthenticatedUser.Id,
            Email = AuthenticatedUser.Email,
            FirstName = "Old",
            LastName = "Name"
        };

        UserManagerMock
            .Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(u => u.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var dto = new UpdateProfileDto
        {
            FirstName = "New",
            LastName = "Name"
        };

        var result = await Controller.UpdateProfile(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value!;

        var firstName = value.GetType().GetProperty("FirstName")?.GetValue(value);
        var lastName = value.GetType().GetProperty("LastName")?.GetValue(value);
        var email = value.GetType().GetProperty("Email")?.GetValue(value);

        Assert.Equal("New", firstName);
        Assert.Equal("Name", lastName);
        Assert.Equal(AuthenticatedUser.Email, email);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnOk_WhenSuccess()
    {
        SetupController();

        var user = new AppUser { Id = AuthenticatedUser.Id };

        UserManagerMock
            .Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(u => u.ChangePasswordAsync(user, "old", "new"))
            .ReturnsAsync(IdentityResult.Success);

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new"
        };

        var result = await Controller.ChangePassword(dto);

        AssertOkResult(result);
    }
}
