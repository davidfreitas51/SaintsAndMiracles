using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Infrastructure.Services;
using Moq;

namespace Infrastructure.Tests.Services;

public class ReligiousOrdersServiceTests
{
    private ReligiousOrdersService CreateService(
        out Mock<IReligiousOrdersRepository> ordersRepo,
        out Mock<IRecentActivityRepository> activityRepo)
    {
        ordersRepo = new Mock<IReligiousOrdersRepository>();
        activityRepo = new Mock<IRecentActivityRepository>();

        return new ReligiousOrdersService(
            ordersRepo.Object,
            activityRepo.Object
        );
    }

    [Fact]
    public async Task CreateReligiousOrderAsync_ShouldReturnOrderAndLogActivity()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var dto = new NewReligiousOrderDto { Name = "Order1" };
        var order = new ReligiousOrder { Id = 1, Name = dto.Name };
        ordersRepo.Setup(r => r.CreateAsync(It.IsAny<ReligiousOrder>())).ReturnsAsync(true)
                  .Callback<ReligiousOrder>(o => o.Id = 1);

        var result = await service.CreateReligiousOrderAsync(dto, "user1");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Order1", result.Name);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 1, "Order1", ActivityAction.Created, "user1"), Times.Once);
    }

    [Fact]
    public async Task CreateReligiousOrderAsync_ShouldReturnNull_WhenCreationFails()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var dto = new NewReligiousOrderDto { Name = "FailOrder" };
        ordersRepo.Setup(r => r.CreateAsync(It.IsAny<ReligiousOrder>())).ReturnsAsync(false);

        var result = await service.CreateReligiousOrderAsync(dto, "user1");

        Assert.Null(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReligiousOrderAsync_ShouldReturnFalse_WhenOrderNotFound()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        ordersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ReligiousOrder?)null);

        var dto = new NewReligiousOrderDto { Name = "UpdatedName" };
        var result = await service.UpdateReligiousOrderAsync(1, dto, "user1");

        Assert.False(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReligiousOrderAsync_ShouldUpdateAndLogActivity()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var existing = new ReligiousOrder { Id = 1, Name = "OldName" };
        ordersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        ordersRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(true);

        var dto = new NewReligiousOrderDto { Name = "NewName" };
        var result = await service.UpdateReligiousOrderAsync(1, dto, "user1");

        Assert.True(result);
        Assert.Equal("NewName", existing.Name);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 1, "NewName", ActivityAction.Updated, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteReligiousOrderAsync_ShouldReturnFalse_WhenOrderNotFound()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        ordersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ReligiousOrder?)null);

        var result = await service.DeleteReligiousOrderAsync(1, "user1");

        Assert.False(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteReligiousOrderAsync_ShouldDeleteAndLogActivity()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var existing = new ReligiousOrder { Id = 1, Name = "OrderToDelete" };
        ordersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        ordersRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask); // <- Corrigido

        var result = await service.DeleteReligiousOrderAsync(1, "user1");

        Assert.True(result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 1, "OrderToDelete", ActivityAction.Deleted, "user1"), Times.Once);
    }

}
