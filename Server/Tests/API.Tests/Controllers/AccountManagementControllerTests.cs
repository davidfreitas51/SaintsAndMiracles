using System.Security.Claims;
using API.Controllers;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace API.Tests.Controllers;

public class AccountManagementControllerTests
{
    private readonly Mock<UserManager<AppUser>> _userManager;
    private readonly Mock<SignInManager<AppUser>> _signInManager;
    private readonly Mock<IEmailSender<AppUser>> _emailSender;
    private readonly IConfiguration _configuration;

    private readonly AccountManagementController _controller;

    public AccountManagementControllerTests()
    {
        _userManager = MockUserManager();
        _signInManager = MockSignInManager(_userManager.Object);
        _emailSender = new Mock<IEmailSender<AppUser>>();

        var configValues = new Dictionary<string, string?>
        {
            { "Frontend:BaseUrl", "http://localhost:3000" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues!)
            .Build();

        _controller = new AccountManagementController(
            _userManager.Object,
            _signInManager.Object,
            _emailSender.Object,
            _configuration
        );
    }

    // ===================== HELPERS =====================

    private static Mock<UserManager<AppUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<AppUser>>();
        return new Mock<UserManager<AppUser>>(
            store.Object,
            null!, null!, null!, null!, null!, null!, null!, null!
        );
    }

    private static Mock<SignInManager<AppUser>> MockSignInManager(UserManager<AppUser> userManager)
    {
        return new Mock<SignInManager<AppUser>>(
            userManager,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<AppUser>>(),
            null!, null!, null!, null!
        );
    }

    private static ClaimsPrincipal CreateUserPrincipal(string userId, string email, string role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private void SetUserContext(ClaimsPrincipal user)
    {
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    // ===================== TESTS =====================

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUser_WhenAuthenticated()
    {
        var user = new AppUser
        {
            Id = "1",
            Email = "test@test.com",
            FirstName = "John",
            LastName = "Doe"
        };

        _userManager
            .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        SetUserContext(CreateUserPrincipal("1", user.Email!, "User"));

        var result = await _controller.GetCurrentUser();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CurrentUserDto>(ok.Value);

        Assert.Equal("John", dto.FirstName);
        Assert.Equal("Doe", dto.LastName);
        Assert.Equal("test@test.com", dto.Email);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenNoUser()
    {
        _userManager
            .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((AppUser?)null);

        SetUserContext(new ClaimsPrincipal());

        var result = await _controller.GetCurrentUser();

        Assert.IsType<UnauthorizedResult>(result.Result);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnOk_WhenSuperAdmin()
    {
        var user = new AppUser
        {
            Email = "admin@test.com",
            FirstName = "Admin",
            LastName = "Root"
        };

        _userManager
            .Setup(u => u.GetUsersInRoleAsync("SuperAdmin"))
            .ReturnsAsync(new List<AppUser> { user });

        _userManager
            .Setup(u => u.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "SuperAdmin" });

        SetUserContext(CreateUserPrincipal("1", user.Email!, "SuperAdmin"));

        var result = await _controller.GetUsers();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsAssignableFrom<IEnumerable<object>>(ok.Value);

        Assert.Single(list);
    }

    [Fact]
    public async Task UpdateProfile_ShouldUpdateNames_WhenValid()
    {
        var user = new AppUser
        {
            Id = "1",
            Email = "test@test.com",
            FirstName = "Old",
            LastName = "Name"
        };

        _userManager
            .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _userManager
            .Setup(u => u.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        SetUserContext(CreateUserPrincipal("1", user.Email!, "User"));

        var dto = new UpdateProfileDto
        {
            FirstName = "New",
            LastName = "Name"
        };

        var result = await _controller.UpdateProfile(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value!;

        var firstName = value.GetType().GetProperty("FirstName")?.GetValue(value);
        var lastName = value.GetType().GetProperty("LastName")?.GetValue(value);
        var email = value.GetType().GetProperty("Email")?.GetValue(value);

        Assert.Equal("New", firstName);
        Assert.Equal("Name", lastName);
        Assert.Equal("test@test.com", email);
    }

    [Fact]
    public async Task ChangePassword_ShouldReturnOk_WhenSuccess()
    {
        var user = new AppUser { Id = "1" };

        _userManager
            .Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(user);

        _userManager
            .Setup(u => u.ChangePasswordAsync(user, "old", "new"))
            .ReturnsAsync(IdentityResult.Success);

        SetUserContext(CreateUserPrincipal("1", "test@test.com", "User"));

        var dto = new ChangePasswordDto
        {
            CurrentPassword = "old",
            NewPassword = "new"
        };

        var result = await _controller.ChangePassword(dto);

        Assert.IsType<OkObjectResult>(result);
    }
}
