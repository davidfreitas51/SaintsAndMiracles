using API.Controllers;
using Core.DTOs;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using System.Security.Claims;

namespace API.Tests.Controllers;

public class RegistrationControllerTests
{
    #region Setup

    private static RegistrationController CreateController(
        out Mock<UserManager<AppUser>> userManagerMock,
        out Mock<SignInManager<AppUser>> signInManagerMock,
        out Mock<IAccountTokensService> tokensServiceMock,
        out Mock<IEmailSender<AppUser>> emailSenderMock,
        bool authenticated = false,
        bool isSuperAdmin = false)
    {
        var store = new Mock<IUserStore<AppUser>>();

        userManagerMock = new Mock<UserManager<AppUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!
        );

        signInManagerMock = new Mock<SignInManager<AppUser>>(
            userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
            null!, null!, null!, null!
        );

        tokensServiceMock = new Mock<IAccountTokensService>();
        emailSenderMock = new Mock<IEmailSender<AppUser>>();

        var controller = new RegistrationController(
            signInManagerMock.Object,
            tokensServiceMock.Object,
            emailSenderMock.Object
        );

        var claims = new List<Claim>();

        if (authenticated)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, "user-1"));
            claims.Add(new Claim(ClaimTypes.Name, "user@test.com"));

            if (isSuperAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
        }

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, authenticated ? "TestAuth" : null)
            ),
            Request =
            {
                Scheme = "http",
                Host = new HostString("localhost")
            }
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // ✅ Mock de IUrlHelper (sem UrlHelper concreto)
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock
            .Setup(u => u.Action(It.IsAny<UrlActionContext>()))
            .Returns("http://localhost/confirm");

        controller.Url = urlHelperMock.Object;

        return controller;
    }

    #endregion

    #region Factories

    private static RegisterDto CreateRegisterDto() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@test.com",
        Password = "Password123!",
        ConfirmPassword = "Password123!",
        InviteToken = "invite-token"
    };


    private static ResendConfirmationDto CreateResendDto() => new()
    {
        Email = "john@test.com"
    };

    #endregion

    #region REGISTER

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenTokenInvalid()
    {
        var controller = CreateController(
            out _, out _, out var tokens, out _
        );

        tokens.Setup(t => t.GetValidTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((AccountToken?)null);

        var result = await controller.Register(CreateRegisterDto());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenEmailExists()
    {
        var controller = CreateController(
            out var userManager, out _, out var tokens, out _
        );

        tokens.Setup(t => t.GetValidTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new AccountToken { Role = "Admin" });

        userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new AppUser());

        var result = await controller.Register(CreateRegisterDto());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_ShouldReturnCreated_WhenSuccessful()
    {
        var controller = CreateController(
            out var userManager,
            out _,
            out var tokens,
            out var emailSender
        );

        tokens.Setup(t => t.GetValidTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(new AccountToken { Role = "Admin" });

        userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((AppUser?)null);

        userManager.Setup(u => u.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        userManager.Setup(u => u.AddToRoleAsync(It.IsAny<AppUser>(), "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        userManager.Setup(u => u.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>()))
            .ReturnsAsync("email-token");

        tokens.Setup(t => t.ConsumeAsync(It.IsAny<string>()))
            .ReturnsAsync(true); // ✅ Task<bool>

        emailSender.Setup(e =>
            e.SendConfirmationLinkAsync(
                It.IsAny<AppUser>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ))
            .Returns(Task.CompletedTask);

        var result = await controller.Register(CreateRegisterDto());

        Assert.IsType<CreatedResult>(result);
    }

    #endregion
}
