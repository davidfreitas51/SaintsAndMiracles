using System.Security.Claims;
using Core.DTOs;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountManagementController(UserManager<AppUser> userManager) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public ActionResult Me()
    {
        return Ok();
    }

    
    [Authorize]
    [HttpGet("current-user")]
    public async Task<ActionResult<CurrentUserDto>> GetCurrentUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (email == null)
            return Unauthorized();

        var user = await userManager.Users
            .Where(u => u.Email == email)
            .Select(u => new CurrentUserDto
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }

}
