using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Core.Models;

namespace Infrastructure.Services;

public class ReligiousOrdersService(
    IReligiousOrdersRepository religiousOrdersRepository,
    IRecentActivityRepository recentActivityRepository
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

            return order;
        }

        return null;
    }

    public async Task<bool> UpdateReligiousOrderAsync(int id, NewReligiousOrderDto dto, string userId)
    {
        var order = await religiousOrdersRepository.GetByIdAsync(id);
        if (order is null) return false;

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
        }

        return updated;
    }

    public async Task<bool> DeleteReligiousOrderAsync(int id, string userId)
    {
        var order = await religiousOrdersRepository.GetByIdAsync(id);
        if (order is null) return false;

        await religiousOrdersRepository.DeleteAsync(id);

        await recentActivityRepository.LogActivityAsync(
            EntityType.ReligiousOrder,
            order.Id,
            order.Name,
            ActivityAction.Deleted,
            userId
        );

        return true;
    }
}
