using API.Controllers;
using Core.DTOs;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Common;

namespace API.Tests.Controllers;

public class RegistrationControllerTests : ControllerTestBase<RegistrationController>
{
    private Mock<IAccountTokensService> _tokensServiceMock = null!;
    private Mock<IEmailSender<AppUser>> _emailSenderMock = null!;

    private void SetupController(bool authenticated = false)
    {
        _tokensServiceMock = CreateLooseMock<IAccountTokensService>();
        _emailSenderMock = CreateLooseMock<IEmailSender<AppUser>>();

        if (authenticated)
        {
            SetupAuthenticatedController((userManager, signInManager) =>
                new RegistrationController(
                    signInManager.Object,
                    _tokensServiceMock.Object,
                    _emailSenderMock.Object,
                    GetNullLogger<RegistrationController>()));
        }
        else
        {
            SetupUnauthenticatedController((userManager, signInManager) =>
                new RegistrationController(
                    signInManager.Object,
                    _tokensServiceMock.Object,
                    _emailSenderMock.Object,
                    GetNullLogger<RegistrationController>()));
        }
    }

    private static RegisterDto CreateRegisterDto() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@test.com",
        Password = "Password123!",
        ConfirmPassword = "Password123!",
        InviteToken = "invite-token"
    };

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenTokenInvalid()
    {
        SetupController();

        _tokensServiceMock.Setup(t => t.GetValidTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((AccountToken?)null);

        var result = await Controller.Register(CreateRegisterDto());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailExists()
    {
        SetupController();

        _tokensServiceMock.Setup(t => t.GetValidTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new AccountToken { Role = "Admin" });

        UserManagerMock.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" }
            ));

        var dto = CreateRegisterDto();

        var result = await Controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var apiError = Assert.IsType<ApiErrorResponse>(badRequest.Value);
        Assert.Contains("Email already exists", apiError.Details);
    }


    [Fact]
    public async Task Register_ShouldReturnCreated_WhenSuccessful()
    {
        SetupController();

        _tokensServiceMock.Setup(t => t.GetValidTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new AccountToken { Role = "Admin" });

        UserManagerMock.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        UserManagerMock.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        UserManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<AppUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        UserManagerMock.Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>()))
            .ReturnsAsync("email-token");

        _tokensServiceMock.Setup(t => t.ConsumeAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        _emailSenderMock.Setup(e =>
            e.SendConfirmationLinkAsync(
                It.IsAny<AppUser>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ))
            .Returns(Task.CompletedTask);

        var result = await Controller.Register(CreateRegisterDto());

        Assert.IsType<CreatedResult>(result);
    }

    // -------------------- EMAIL CONFIRMATION --------------------

    [Fact]
    public async Task ConfirmEmail_ShouldRedirectToSuccess_WhenValid()
    {
        SetupController();

        var user = TestDataFactory.CreateDefaultUser();
        var token = "valid-token";

        var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        envMock.SetupGet(e => e.EnvironmentName).Returns("Production");

        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c["Frontend:BaseUrl"]).Returns("http://test-frontend");

        UserManagerMock
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(x => x.ConfirmEmailAsync(user, token))
            .ReturnsAsync(IdentityResult.Success);

        var result = await Controller.ConfirmEmail(user.Id, token, envMock.Object, configMock.Object);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Contains("success=true", redirect.Url);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldRedirectToFailure_WhenUserNotFound()
    {
        SetupController();

        var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        envMock.SetupGet(e => e.EnvironmentName).Returns("Production");

        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c["Frontend:BaseUrl"]).Returns("http://test-frontend");

        UserManagerMock
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        var result = await Controller.ConfirmEmail("invalid-id", "token", envMock.Object, configMock.Object);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Contains("success=false", redirect.Url);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldRedirectToFailure_WhenTokenInvalid()
    {
        SetupController();

        var user = TestDataFactory.CreateDefaultUser();

        var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        envMock.SetupGet(e => e.EnvironmentName).Returns("Production");

        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c["Frontend:BaseUrl"]).Returns("http://test-frontend");

        UserManagerMock
            .Setup(x => x.FindByIdAsync(user.Id))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(x => x.ConfirmEmailAsync(user, It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var result = await Controller.ConfirmEmail(user.Id, "invalid-token", envMock.Object, configMock.Object);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Contains("success=false", redirect.Url);
    }

    // -------------------- RESEND CONFIRMATION --------------------

    [Fact]
    public async Task ResendConfirmation_ShouldReturnOk_WhenValid()
    {
        SetupController();

        var user = TestDataFactory.CreateDefaultUser();
        var dto = new ResendConfirmationDto { Email = user.Email! };

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(x => x.IsEmailConfirmedAsync(user))
            .ReturnsAsync(false);

        UserManagerMock
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
            .ReturnsAsync("confirmation-token");

        var result = await Controller.ResendConfirmation(dto);

        AssertOkResult(result);
        _emailSenderMock.Verify(
            x => x.SendConfirmationLinkAsync(user, user.Email, It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ResendConfirmation_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        SetupController();

        var dto = new ResendConfirmationDto { Email = "nonexistent@test.com" };

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync((AppUser?)null);

        var result = await Controller.ResendConfirmation(dto);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ResendConfirmation_ShouldReturnBadRequest_WhenEmailAlreadyConfirmed()
    {
        SetupController();

        var user = TestDataFactory.CreateDefaultUser();
        var dto = new ResendConfirmationDto { Email = user.Email! };

        UserManagerMock
            .Setup(x => x.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        UserManagerMock
            .Setup(x => x.IsEmailConfirmedAsync(user))
            .ReturnsAsync(true);

        var result = await Controller.ResendConfirmation(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
