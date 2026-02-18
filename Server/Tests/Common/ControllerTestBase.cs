using System.Security.Claims;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
namespace Tests.Common;

/// <summary>
/// Base class for controller tests. Handles authentication setup, mocking, and HTTP context.
/// </summary>
public abstract class ControllerTestBase<TController> : UnitTestBase where TController : ControllerBase
{
    protected TController Controller { get; private set; } = null!;
    protected Mock<UserManager<AppUser>> UserManagerMock { get; private set; } = null!;
    protected Mock<SignInManager<AppUser>> SignInManagerMock { get; private set; } = null!;
    protected AppUser AuthenticatedUser { get; private set; } = null!;

    /// <summary>
    /// Sets up the controller with authenticated user context.
    /// </summary>
    protected void SetupAuthenticatedController(
        Func<Mock<UserManager<AppUser>>, Mock<SignInManager<AppUser>>, TController> controllerFactory,
        string? userId = null,
        string? email = null,
        bool emailConfirmed = true)
    {
        userId ??= "test-user-1";
        email ??= "test@test.com";

        AuthenticatedUser = new AppUser
        {
            Id = userId,
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed
        };

        UserManagerMock = CreateUserManagerMock();
        SignInManagerMock = CreateSignInManagerMock(UserManagerMock);

        Controller = controllerFactory(UserManagerMock, SignInManagerMock);
        AttachAuthenticationContext(AuthenticatedUser);

        // Setup GetUserAsync to return the authenticated user
        UserManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(AuthenticatedUser);

        // Setup GetUserId to return the user ID
        UserManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns(AuthenticatedUser.Id);
    }

    /// <summary>
    /// Sets up the controller with unauthenticated user context.
    /// </summary>
    protected void SetupUnauthenticatedController(
        Func<Mock<UserManager<AppUser>>, Mock<SignInManager<AppUser>>, TController> controllerFactory)
    {
        UserManagerMock = CreateUserManagerMock();
        SignInManagerMock = CreateSignInManagerMock(UserManagerMock);

        Controller = controllerFactory(UserManagerMock, SignInManagerMock);
        AttachUnauthenticatedContext();

        // Setup GetUserAsync to return null for unauthenticated
        UserManagerMock.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((AppUser?)null);

        // Setup GetUserId to return null for unauthenticated
        UserManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
            .Returns((string?)null);
    }

    /// <summary>
    /// Attaches HTTP context with authenticated user claims to the controller.
    /// </summary>
    private void AttachAuthenticationContext(AppUser user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? "")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        SetupUrlHelper();
    }

    /// <summary>
    /// Attaches HTTP context with no authentication to the controller.
    /// </summary>
    private void AttachUnauthenticatedContext()
    {
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        Controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        SetupUrlHelper();
    }

    /// <summary>
    /// Sets up a mock IUrlHelper to prevent null reference issues.
    /// </summary>
    private void SetupUrlHelper()
    {
        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("https://test-url/action");

        Controller.Url = urlHelperMock.Object;
    }

    /// <summary>
    /// Creates a properly configured UserManager mock.
    /// </summary>
    private Mock<UserManager<AppUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            store.Object,
            null, null, null, null, null, null, null, null);
    }

    /// <summary>
    /// Creates a properly configured SignInManager mock.
    /// </summary>
    private Mock<SignInManager<AppUser>> CreateSignInManagerMock(Mock<UserManager<AppUser>> userManagerMock)
    {
        return new Mock<SignInManager<AppUser>>(
            userManagerMock.Object,
            new HttpContextAccessor(),
            new Mock<IUserClaimsPrincipalFactory<AppUser>>().Object,
            Microsoft.Extensions.Options.Options.Create(new IdentityOptions()),
            new Mock<Microsoft.Extensions.Logging.ILogger<SignInManager<AppUser>>>().Object,
            new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>().Object,
            new Mock<Microsoft.AspNetCore.Identity.IUserConfirmation<AppUser>>().Object
        );
    }

    /// <summary>
    /// Gets the currently authenticated user from the controller context.
    /// </summary>
    protected string? GetCurrentUserId()
        => Controller.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    /// <summary>
    /// Asserts that the result is an OkObjectResult.
    /// </summary>
    protected void AssertOkResult(IActionResult result)
    {
        Assert.IsType<OkObjectResult>(result);
    }

    /// <summary>
    /// Asserts that the result is an OkObjectResult with the specified content.
    /// </summary>
    protected void AssertOkResult<T>(IActionResult result, T? expectedContent) where T : class
    {
        Assert.IsType<OkObjectResult>(result);
        if (expectedContent != null)
        {
            var okResult = result as OkObjectResult;
            Assert.Equal(expectedContent, okResult?.Value);
        }
    }

    /// <summary>
    /// Asserts that the result is a BadRequestObjectResult.
    /// </summary>
    protected void AssertBadRequest(IActionResult result)
        => Assert.IsType<BadRequestObjectResult>(result);

    /// <summary>
    /// Asserts that the result is an Unauthorized response (either UnauthorizedResult or UnauthorizedObjectResult).
    /// </summary>
    protected void AssertUnauthorized(IActionResult result)
    {
        Assert.True(
            result is UnauthorizedResult || result is UnauthorizedObjectResult,
            $"Expected Unauthorized result but got {result.GetType().Name}"
        );
    }

    /// <summary>
    /// Asserts that the result is a NotFoundResult.
    /// </summary>
    protected void AssertNotFound(IActionResult result)
        => Assert.IsType<NotFoundResult>(result);

    /// <summary>
    /// Asserts that the result is a NoContentResult.
    /// </summary>
    protected void AssertNoContent(IActionResult result)
        => Assert.IsType<NoContentResult>(result);

    /// <summary>
    /// Asserts that the result is a ConflictObjectResult.
    /// </summary>
    protected void AssertConflict(IActionResult result)
        => Assert.IsType<ConflictObjectResult>(result);
}
