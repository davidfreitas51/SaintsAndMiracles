using Core.DTOs;
using Core.Enums;
using Core.Interfaces;
using Core.Interfaces.Repositories;
using Core.Models;
using Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Infrastructure.Tests.Services;

/// <summary>
/// Tests for ReligiousOrdersService CRUD operations.
/// </summary>
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
            activityRepo.Object,
            NullLogger<ReligiousOrdersService>.Instance
        );
    }

    // ==================== CREATE ORDER ====================

    [Fact]
    public async Task CreateReligiousOrderAsync_ShouldReturnOrder_WhenCreationSucceeds()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var dto = new NewReligiousOrderDto { Name = "Franciscans" };
        ordersRepo.Setup(r => r.CreateAsync(It.IsAny<ReligiousOrder>()))
            .ReturnsAsync(true)
            .Callback<ReligiousOrder>(o => o.Id = 1);

        var result = await service.CreateReligiousOrderAsync(dto, "user1");

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Franciscans", result.Name);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 1, "Franciscans", ActivityAction.Created, "user1"), Times.Once);
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
    public async Task CreateReligiousOrderAsync_ShouldCreateDominicanOrder()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var dto = new NewReligiousOrderDto { Name = "Dominicans" };
        ordersRepo.Setup(r => r.CreateAsync(It.IsAny<ReligiousOrder>()))
            .ReturnsAsync(true)
            .Callback<ReligiousOrder>(o => o.Id = 2);

        var result = await service.CreateReligiousOrderAsync(dto, "user1");

        Assert.NotNull(result);
        Assert.Equal("Dominicans", result!.Name);
    }

    [Fact]
    public async Task CreateReligiousOrderAsync_ShouldCreateCarmeliteOrder()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var dto = new NewReligiousOrderDto { Name = "Carmelites" };
        ordersRepo.Setup(r => r.CreateAsync(It.IsAny<ReligiousOrder>()))
            .ReturnsAsync(true)
            .Callback<ReligiousOrder>(o => o.Id = 3);

        var result = await service.CreateReligiousOrderAsync(dto, "user1");

        Assert.NotNull(result);
        Assert.Equal("Carmelites", result!.Name);
    }

    [Fact]
    public async Task CreateReligiousOrderAsync_ShouldCreateForAnonymousUser()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var dto = new NewReligiousOrderDto { Name = "Benedictines" };
        ordersRepo.Setup(r => r.CreateAsync(It.IsAny<ReligiousOrder>()))
            .ReturnsAsync(true)
            .Callback<ReligiousOrder>(o => o.Id = 4);

        var result = await service.CreateReligiousOrderAsync(dto, null);

        Assert.NotNull(result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 4, "Benedictines", ActivityAction.Created, null), Times.Once);
    }

    // ==================== UPDATE ORDER ====================

    [Fact]
    public async Task UpdateReligiousOrderAsync_ShouldReturnFalse_WhenOrderNotFound()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        ordersRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ReligiousOrder?)null);

        var dto = new NewReligiousOrderDto { Name = "UpdatedName" };
        var result = await service.UpdateReligiousOrderAsync(999, dto, "user1");

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
    public async Task UpdateReligiousOrderAsync_ShouldReturnFalse_WhenUpdateFails()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var existing = new ReligiousOrder { Id = 1, Name = "Original" };
        ordersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        ordersRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(false);

        var dto = new NewReligiousOrderDto { Name = "NewName" };
        var result = await service.UpdateReligiousOrderAsync(1, dto, "user1");

        // Service modifies object before calling UpdateAsync, so object will be changed even if update fails
        // What matters is that UpdateAsync returns false and activity is not logged
        Assert.False(result);
        Assert.Equal("NewName", existing.Name); // Object was modified by service
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReligiousOrderAsync_ShouldUpdateDominicanOrder()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var existing = new ReligiousOrder { Id = 2, Name = "Dominicans" };
        ordersRepo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(existing);
        ordersRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(true);

        var dto = new NewReligiousOrderDto { Name = "Order of Preachers" };
        var result = await service.UpdateReligiousOrderAsync(2, dto, "user1");

        Assert.True(result);
        Assert.Equal("Order of Preachers", existing.Name);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 2, "Order of Preachers", ActivityAction.Updated, "user1"), Times.Once);
    }

    [Fact]
    public async Task UpdateReligiousOrderAsync_ShouldUpdateForAnonymousUser()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var existing = new ReligiousOrder { Id = 5, Name = "OldAnon" };
        ordersRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(existing);
        ordersRepo.Setup(r => r.UpdateAsync(existing)).ReturnsAsync(true);

        var dto = new NewReligiousOrderDto { Name = "NewAnon" };
        var result = await service.UpdateReligiousOrderAsync(5, dto, null);

        Assert.True(result);
        Assert.Equal("NewAnon", existing.Name);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 5, "NewAnon", ActivityAction.Updated, null), Times.Once);
    }

    // ==================== DELETE ORDER ====================

    [Fact]
    public async Task DeleteReligiousOrderAsync_ShouldReturnFalse_WhenOrderNotFound()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        ordersRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((ReligiousOrder?)null);

        var result = await service.DeleteReligiousOrderAsync(999, "user1");

        Assert.False(result);
        activityRepo.Verify(a => a.LogActivityAsync(It.IsAny<EntityType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ActivityAction>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteReligiousOrderAsync_ShouldDeleteAndLogActivity()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var existing = new ReligiousOrder { Id = 1, Name = "OrderToDelete" };
        ordersRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        ordersRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        var result = await service.DeleteReligiousOrderAsync(1, "user1");

        Assert.True(result);
        ordersRepo.Verify(r => r.DeleteAsync(1), Times.Once);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 1, "OrderToDelete", ActivityAction.Deleted, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteReligiousOrderAsync_ShouldDeleteFranciscanOrder()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var existing = new ReligiousOrder { Id = 3, Name = "Franciscans" };
        ordersRepo.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(existing);
        ordersRepo.Setup(r => r.DeleteAsync(3)).Returns(Task.CompletedTask);

        var result = await service.DeleteReligiousOrderAsync(3, "user1");

        Assert.True(result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 3, "Franciscans", ActivityAction.Deleted, "user1"), Times.Once);
    }

    [Fact]
    public async Task DeleteReligiousOrderAsync_ShouldDeleteForAnonymousUser()
    {
        var service = CreateService(out var ordersRepo, out var activityRepo);

        var existing = new ReligiousOrder { Id = 10, Name = "AnonOrderDelete" };
        ordersRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(existing);
        ordersRepo.Setup(r => r.DeleteAsync(10)).Returns(Task.CompletedTask);

        var result = await service.DeleteReligiousOrderAsync(10, null);

        Assert.True(result);
        activityRepo.Verify(a => a.LogActivityAsync(EntityType.ReligiousOrder, 10, "AnonOrderDelete", ActivityAction.Deleted, null), Times.Once);
    }
}
