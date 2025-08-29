using System.Security.Claims;
using Core.DTOs;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountManagementController(IAccountManagementService accountManagementService) : ControllerBase
{
    [Authorize]
    [HttpGet("current-user")]
    public ActionResult<CurrentUserDto> GetCurrentUser()
    {
        return new CurrentUserDto
        {
            FirstName = User.FindFirst("firstName")?.Value ?? "",
            LastName = User.FindFirst("lastName")?.Value ?? "",
            Email = User.FindFirst(ClaimTypes.Email)?.Value ?? ""
        };
    }
}
