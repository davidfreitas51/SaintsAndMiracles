using Core.DTOs;
using Xunit;

namespace Core.Tests.DTOs;

public class DashboardSummaryDtoTests
{
    [Fact]
    public void Should_Have_DefaultValues_AsZero()
    {
        var dto = new DashboardSummaryDto();

        Assert.Equal(0, dto.TotalSaints);
        Assert.Equal(0, dto.TotalMiracles);
        Assert.Equal(0, dto.TotalPrayers);
        Assert.Equal(0, dto.TotalAccounts);
    }

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
}
