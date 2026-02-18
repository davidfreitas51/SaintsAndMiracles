using API.Controllers;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Common;
using Tests.Common.Builders;

namespace API.Tests.Controllers;

/// <summary>
/// Test suite for AuthenticationController.
/// Tests login, logout, and password reset functionality.
/// </summary>
public class AuthenticationControllerTests : ControllerTestBase<AuthenticationController>
{
    private Mock<IEmailSender<AppUser>> _emailSenderMock = null!;

    /// <summary>
    /// Helper to setup controller with mocked dependencies.
    /// </summary>
    private void SetupController()
    {
        _emailSenderMock = CreateLooseMock<IEmailSender<AppUser>>();

        SetupAuthenticatedController(
            (userManager, signInManager) =>
            {
                return new AuthenticationController(
                    signInManager.Object,
                    _emailSenderMock.Object,
                    GetNullLogger<AuthenticationController>()
                );
            }
        );
    }

    // -------------------- LOGIN --------------------

    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        SetupController();

        var dto = LoginDtoBuilder.Default().Build();

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null);

        var result = await Controller.Login(dto);

        AssertUnauthorized(result);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        SetupController();

        var user = TestDataFactory.CreateDefaultUser();
        var dto = LoginDtoBuilder.Default()
            .WithEmail(user.Email!)
            .Build();

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(x => x.IsEmailConfirmedAsync(user))
            .ReturnsAsync(true);

        SignInManagerMock
            .Setup(s => s.PasswordSignInAsync(user, dto.Password, dto.RememberMe, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

        var result = await Controller.Login(dto);

        AssertOkResult(result);
    }

    [Fact]
    public async Task Login_EmailNotConfirmed_ReturnsUnauthorized_AndSendsEmail()
    {
        SetupController();

        var user = TestDataFactory.CreateUnconfirmedUser();
        var dto = LoginDtoBuilder.Default()
            .WithEmail(user.Email!)
            .Build();

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(x => x.IsEmailConfirmedAsync(user))
            .ReturnsAsync(false);

        UserManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");

        var result = await Controller.Login(dto);

        AssertUnauthorized(result);
        _emailSenderMock.Verify(
            x => x.SendConfirmationLinkAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Login_LockedOutUser_ReturnsUnauthorized()
    {
        SetupController();

        var user = TestDataFactory.CreateLockedOutUser();
        var dto = LoginDtoBuilder.Default()
            .WithEmail(user.Email!)
            .Build();

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(x => x.IsEmailConfirmedAsync(user))
            .ReturnsAsync(true);

        // PasswordSignInAsync should return LockedOut for locked-out users
        SignInManagerMock
            .Setup(s => s.PasswordSignInAsync(user, dto.Password, dto.RememberMe, false))
            .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.LockedOut);

        var result = await Controller.Login(dto);

        AssertUnauthorized(result);
    }

    // -------------------- LOGOUT --------------------

    [Fact]
    public async Task Logout_CallsSignOut_ReturnsNoContent()
    {
        SetupController();

        var result = await Controller.Logout();

        AssertNoContent(result);
        SignInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
    }

    // -------------------- FORGOT PASSWORD --------------------

    [Fact]
    public async Task ForgotPassword_UserDoesNotExist_ReturnsOk()
    {
        SetupController();

        var dto = new ForgotPasswordDto { Email = "nonexistent@test.com" };

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null);

        var result = await Controller.ForgotPassword(dto);

        AssertOkResult(result);
        _emailSenderMock.Verify(
            x => x.SendPasswordResetLinkAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }

    [Fact]
    public async Task ForgotPassword_ValidEmail_SendsResetEmail()
    {
        SetupController();

        var user = TestDataFactory.CreateDefaultUser();
        var dto = new ForgotPasswordDto { Email = user.Email! };

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(x => x.IsEmailConfirmedAsync(user))
            .ReturnsAsync(true);

        UserManagerMock
            .Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("reset-token");

        var result = await Controller.ForgotPassword(dto);

        AssertOkResult(result);
        _emailSenderMock.Verify(
            x => x.SendPasswordResetLinkAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }
}
