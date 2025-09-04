using Core.DTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class DashboardController(
    ISaintsRepository saintsRepository,
    IMiraclesRepository miraclesRepository,
    IPrayersRepository prayersRepository,
    UserManager<AppUser> userManager) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var totalSaintsTask = await saintsRepository.GetTotalSaintsAsync();
        var totalMiraclesTask = await miraclesRepository.GetTotalMiraclesAsync();
        var totalPrayersTask = await prayersRepository.GetTotalPrayersAsync();
        var totalAccountsTask = await userManager.Users.CountAsync(u => u.EmailConfirmed);

        var summary = new DashboardSummaryDto
        {
            TotalSaints = totalSaintsTask,
            TotalMiracles = totalMiraclesTask,
            TotalPrayers = totalPrayersTask,
            TotalAccounts = totalAccountsTask
        };

        return Ok(summary);
    }
}
