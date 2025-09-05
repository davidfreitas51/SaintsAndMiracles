using Core.DTOs;
using Core.Interfaces;
using Core.Interfaces.Repositories;
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
    IRecentActivityRepository recentActivityRepository,
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

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentActivity(int pageNumber = 1, int pageSize = 10)
    {
        var pagedActivities = await recentActivityRepository.GetRecentActivitiesAsync(pageNumber, pageSize);

        var userIds = pagedActivities.Items
            .Where(a => !string.IsNullOrEmpty(a.UserId))
            .Select(a => a.UserId!)
            .Distinct()
            .ToList();

        var users = await userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var result = pagedActivities.Items.Select(a => new
        {
            a.EntityName,
            a.EntityId,
            a.DisplayName,
            a.Action,
            a.CreatedAt,
            UserEmail = users.FirstOrDefault(u => u.Id == a.UserId)?.Email
        }).ToList();

        return Ok(new
        {
            Items = result,
            pagedActivities.TotalCount,
            pagedActivities.PageNumber,
            pagedActivities.PageSize
        });
    }

}
