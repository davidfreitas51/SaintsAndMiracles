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
        var totalSaintsTask = saintsRepository.GetTotalSaintsAsync();
        var totalMiraclesTask = miraclesRepository.GetTotalMiraclesAsync();
        var totalPrayersTask = prayersRepository.GetTotalPrayersAsync();
        var totalAccountsTask = userManager.Users.CountAsync(u => u.EmailConfirmed);

        await Task.WhenAll(totalSaintsTask, totalMiraclesTask, totalPrayersTask, totalAccountsTask);

        var summary = new DashboardSummaryDto
        {
            TotalSaints = totalSaintsTask.Result,
            TotalMiracles = totalMiraclesTask.Result,
            TotalPrayers = totalPrayersTask.Result,
            TotalAccounts = totalAccountsTask.Result
        };

        return Ok(summary);
    }
}
