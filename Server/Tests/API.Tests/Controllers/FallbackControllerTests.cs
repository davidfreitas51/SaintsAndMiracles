using API.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace API.Tests.Controllers;

public class FallbackControllerTests
{
    [Fact]
    public void Index_ReturnsIndexHtmlPhysicalFile()
    {
        var controller = new FallbackController();

        var result = controller.Index();

        var fileResult = Assert.IsType<PhysicalFileResult>(result);
        var expectedPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");

        Assert.Equal(expectedPath, fileResult.FileName);
        Assert.Equal("text/HTML", fileResult.ContentType);
    }
}
