using API.Controllers;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace API.Tests.Controllers;

public class AuthenticationControllerTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<SignInManager<AppUser>> _signInManagerMock;
    private readonly Mock<IEmailSender<AppUser>> _emailSenderMock;
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTests()
    {
        var userStoreMock = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(
            userStoreMock.Object,
            null, null, null, null, null, null, null, null);

        _signInManagerMock = new Mock<SignInManager<AppUser>>(
            _userManagerMock.Object,
            new HttpContextAccessor(),
            new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
            Options.Create(new IdentityOptions()),
            new Mock<ILogger<SignInManager<AppUser>>>().Object,
            new Mock<IAuthenticationSchemeProvider>().Object,
            new Mock<IUserConfirmation<AppUser>>().Object
        );

        _emailSenderMock = new Mock<IEmailSender<AppUser>>();

        _controller = new AuthenticationController(
            _signInManagerMock.Object,
            _emailSenderMock.Object,
            NullLogger<AuthenticationController>.Instance
        );

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // minimal UrlHelper to avoid null
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("https://fake-link");
        _controller.Url = urlHelperMock.Object;
    }

    [Fact]
    public async Task Login_InvalidEmail_ReturnsUnauthorized()
    {
        var dto = new LoginDto { Email = "notfound@test.com", Password = "123" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser)null);

        var result = await _controller.Login(dto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Login_EmailNotConfirmed_ReturnsUnauthorized_AndSendsEmail()
    {
        var user = new AppUser { Id = "1", Email = "user@test.com" };
        var dto = new LoginDto { Email = user.Email, Password = "123" };

        _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.IsEmailConfirmedAsync(user)).ReturnsAsync(false);
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user)).ReturnsAsync("token");

        var result = await _controller.Login(dto);

        _emailSenderMock.Verify(x => x.SendConfirmationLinkAsync(user, user.Email, "https://fake-link"), Times.Once);
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Logout_CallsSignOut_ReturnsNoContent()
    {
        var result = await _controller.Logout();

        _signInManagerMock.Verify(x => x.SignOutAsync(), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task ForgotPassword_UserDoesNotExist_ReturnsOkWithoutSendingEmail()
    {
        var dto = new ForgotPasswordDto { Email = "notfound@test.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync(dto.Email)).ReturnsAsync((AppUser)null);

        var result = await _controller.ForgotPassword(dto);

        _emailSenderMock.Verify(x => x.SendPasswordResetLinkAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        Assert.IsType<OkObjectResult>(result);
    }
}
