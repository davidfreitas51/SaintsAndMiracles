using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

/// <summary>
/// Tests for DashboardSummaryDto validation.
/// Ensures dashboard summary data structure and default values work correctly.
/// </summary>
public class DashboardSummaryDtoTests
{
    // ==================== DEFAULT VALUES ====================

    [Fact]
    public void Should_Have_DefaultValuesAsZero()
    {
        var dto = new DashboardSummaryDto();

        Assert.Equal(0, dto.TotalSaints);
        Assert.Equal(0, dto.TotalMiracles);
        Assert.Equal(0, dto.TotalPrayers);
        Assert.Equal(0, dto.TotalAccounts);
    }

    // ==================== VALUE ASSIGNMENT ====================

    [Fact]
    public void Should_Assign_Values_Correctly()
    {
        var dto = new DashboardSummaryDto
        {
            TotalSaints = 10,
            TotalMiracles = 5,
            TotalPrayers = 20,
            TotalAccounts = 3
        };

        Assert.Equal(10, dto.TotalSaints);
        Assert.Equal(5, dto.TotalMiracles);
        Assert.Equal(20, dto.TotalPrayers);
        Assert.Equal(3, dto.TotalAccounts);
    }

    [Fact]
    public void Should_Be_Valid_Model()
    {
        var dto = new DashboardSummaryDto
        {
            TotalSaints = 1,
            TotalMiracles = 1,
            TotalPrayers = 1,
            TotalAccounts = 1
        };

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }

    // ==================== EDGE CASES ====================

    [Fact]
    public void Should_Handle_Large_Values()
    {
        var dto = new DashboardSummaryDto
        {
            TotalSaints = int.MaxValue,
            TotalMiracles = 1000000,
            TotalPrayers = 5000000,
            TotalAccounts = 50000
        };

        Assert.Equal(int.MaxValue, dto.TotalSaints);
        Assert.Equal(1000000, dto.TotalMiracles);
        Assert.Equal(5000000, dto.TotalPrayers);
        Assert.Equal(50000, dto.TotalAccounts);
    }

    [Fact]
    public void Should_Handle_Negative_Values()
    {
        var dto = new DashboardSummaryDto
        {
            TotalSaints = -1,
            TotalMiracles = -5,
            TotalPrayers = -20,
            TotalAccounts = -3
        };

        Assert.Equal(-1, dto.TotalSaints);
        Assert.Equal(-5, dto.TotalMiracles);
        Assert.Equal(-20, dto.TotalPrayers);
        Assert.Equal(-3, dto.TotalAccounts);
    }

    [Fact]
    public void Should_Update_Values_After_Creation()
    {
        var dto = new DashboardSummaryDto
        {
            TotalSaints = 5,
            TotalMiracles = 2,
            TotalPrayers = 10,
            TotalAccounts = 1
        };

        dto.TotalSaints = 10;
        dto.TotalMiracles = 5;

        Assert.Equal(10, dto.TotalSaints);
        Assert.Equal(5, dto.TotalMiracles);
        Assert.Equal(10, dto.TotalPrayers);
        Assert.Equal(1, dto.TotalAccounts);
    }

    // ==================== VALIDATION ====================

    [Fact]
    public void Should_Always_Be_Valid_ReadModel()
    {
        var dto = new DashboardSummaryDto();

        var results = ModelValidationHelper.Validate(dto);

        Assert.Empty(results);
    }
}
