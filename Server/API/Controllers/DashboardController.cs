using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class DashboardController(ISaintsRepository saintsRepository, IMiraclesRepository miraclesRepository, IPrayersRepository prayersRepository, UserManager<AppUser> userManager) : ControllerBase
{
    [HttpGet("saints")]
    public async Task<IActionResult> TotalSaints()
    {
        var totalSaints = await saintsRepository.GetTotalSaintsAsync();
        return Ok(totalSaints);
    }

    [HttpGet("miracles")]
    public async Task<IActionResult> TotalMiracles()
    {
        var totalMiracles = await miraclesRepository.GetTotalMiraclesAsync();
        return Ok(totalMiracles);
    }

    [HttpGet("prayers")]
    public async Task<IActionResult> TotalPrayers()
    {
        var totalPrayers = await prayersRepository.GetTotalPrayersAsync();
        return Ok(totalPrayers);
    }

    [HttpGet("accounts")]
    public async Task<IActionResult> TotalAccounts()
    {
        var totalAccounts = await Task.FromResult(userManager.Users.Count());
        return Ok(totalAccounts);
    }
}
