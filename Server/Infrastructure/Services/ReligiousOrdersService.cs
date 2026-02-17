using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ReligiousOrdersService(
    IReligiousOrdersRepository religiousOrdersRepository,
    IRecentActivityRepository recentActivityRepository,
    ILogger<ReligiousOrdersService> logger
) : IReligiousOrdersService
{

    public async Task<ReligiousOrder?> CreateReligiousOrderAsync(NewReligiousOrderDto dto, string userId)
    {
        var order = new ReligiousOrder
        {
            Name = dto.Name,
        };

        var created = await religiousOrdersRepository.CreateAsync(order);

        if (created)
        {
            await recentActivityRepository.LogActivityAsync(
                EntityType.ReligiousOrder,
                order.Id,
                order.Name,
                ActivityAction.Created,
                userId
            );

            logger.LogInformation("Religious order created successfully. Id={Id}, Name={Name}, UserId={UserId}", order.Id, order.Name, userId);
            return order;
        }

        logger.LogWarning("Religious order creation failed in repository. Name={Name}, UserId={UserId}", dto.Name, userId);
        return null;
    }

    public async Task<bool> UpdateReligiousOrderAsync(int id, NewReligiousOrderDto dto, string userId)
    {
        var order = await religiousOrdersRepository.GetByIdAsync(id);
        if (order is null)
        {
            logger.LogWarning("Update religious order failed: Order not found. Id={Id}, UserId={UserId}", id, userId);
            return false;
        }

        order.Name = dto.Name;

        var updated = await religiousOrdersRepository.UpdateAsync(order);

        if (updated)
        {
            await recentActivityRepository.LogActivityAsync(
                EntityType.ReligiousOrder,
                order.Id,
                order.Name,
                ActivityAction.Updated,
                userId
            );

            logger.LogInformation("Religious order updated successfully. Id={Id}, Name={Name}, UserId={UserId}", id, dto.Name, userId);
        }
        else
        {
            logger.LogWarning("Religious order update failed in repository. Id={Id}, Name={Name}, UserId={UserId}", id, dto.Name, userId);
        }

        return updated;
    }

    public async Task<bool> DeleteReligiousOrderAsync(int id, string userId)
    {
        var order = await religiousOrdersRepository.GetByIdAsync(id);
        if (order is null)
        {
            logger.LogWarning("Delete religious order failed: Order not found. Id={Id}, UserId={UserId}", id, userId);
            return false;
        }

        await religiousOrdersRepository.DeleteAsync(id);

        await recentActivityRepository.LogActivityAsync(
            EntityType.ReligiousOrder,
            order.Id,
            order.Name,
            ActivityAction.Deleted,
            userId
        );

        logger.LogInformation("Religious order deleted successfully. Id={Id}, Name={Name}, UserId={UserId}", id, order.Name, userId);
        return true;
    }
}
