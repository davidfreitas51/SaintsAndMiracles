using Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Infrastructure.Tests.Services;

public class FileStorageServiceTests
{
    private static FileStorageService CreateService(string contentRootPath)
    {
        var env = new Mock<IHostEnvironment>();
        env.SetupGet(x => x.ContentRootPath).Returns(contentRootPath);

        return new FileStorageService(env.Object, NullLogger<FileStorageService>.Instance);
    }

    private static string CreateTempRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "sam-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    [Fact]
    public void GenerateSlug_ShouldNormalizeText()
    {
        var root = CreateTempRoot();
        try
        {
            var service = CreateService(root);

            var slug = service.GenerateSlug("Saint François 2026!");

            Assert.Equal("saint-fran-ois-2026", slug);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task DeleteFolderAsync_ShouldDeleteExistingFolder()
    {
        var root = CreateTempRoot();
        try
        {
            var service = CreateService(root);
            var folderPath = Path.Combine(root, "wwwroot", "saints", "saint-francis");
            Directory.CreateDirectory(folderPath);

            await service.DeleteFolderAsync("saints", "saint-francis");

            Assert.False(Directory.Exists(folderPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RenameFolderIfNeededAsync_ShouldMoveFolder_WhenOldFolderExists()
    {
        var root = CreateTempRoot();
        try
        {
            var service = CreateService(root);
            var baseFolder = Path.Combine(root, "wwwroot", "saints");
            var oldFolder = Path.Combine(baseFolder, "old-slug");
            var newFolder = Path.Combine(baseFolder, "new-slug");

            Directory.CreateDirectory(oldFolder);
            await File.WriteAllTextAsync(Path.Combine(oldFolder, "markdown.md"), "content");

            await service.RenameFolderIfNeededAsync("saints", "old-slug", "new-slug");

            Assert.False(Directory.Exists(oldFolder));
            Assert.True(Directory.Exists(newFolder));
            Assert.True(File.Exists(Path.Combine(newFolder, "markdown.md")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task RenameFolderIfNeededAsync_ShouldCreateNewFolder_WhenOldFolderMissing()
    {
        var root = CreateTempRoot();
        try
        {
            var service = CreateService(root);
            var newFolder = Path.Combine(root, "wwwroot", "saints", "new-slug");

            await service.RenameFolderIfNeededAsync("saints", "missing-old", "new-slug");

            Assert.True(Directory.Exists(newFolder));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldWriteMarkdownAndReturnRelativePaths_WhenImageIsMissing()
    {
        var root = CreateTempRoot();
        try
        {
            var service = CreateService(root);

            var (markdownPath, imagePath) = await service.SaveFilesAsync(
                "saints",
                "saint-francis",
                "# Saint Francis",
                null
            );

            Assert.Equal("/saints/saint-francis/markdown.md", markdownPath);
            Assert.Null(imagePath);

            var fullMarkdownPath = Path.Combine(root, "wwwroot", "saints", "saint-francis", "markdown.md");
            Assert.True(File.Exists(fullMarkdownPath));
            Assert.Equal("# Saint Francis", await File.ReadAllTextAsync(fullMarkdownPath));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldMoveExistingImage_WhenExistingImagePathProvided()
    {
        var root = CreateTempRoot();
        try
        {
            var service = CreateService(root);
            var oldImageFolder = Path.Combine(root, "wwwroot", "saints", "old-slug");
            Directory.CreateDirectory(oldImageFolder);

            var oldImagePath = Path.Combine(oldImageFolder, "image.png");
            await File.WriteAllBytesAsync(oldImagePath, new byte[] { 1, 2, 3, 4 });

            var (markdownPath, imagePath) = await service.SaveFilesAsync(
                "saints",
                "new-slug",
                "updated markdown",
                null,
                "/saints/old-slug/image.png"
            );

            Assert.Equal("/saints/new-slug/markdown.md", markdownPath);
            Assert.Equal("/saints/new-slug/image.png", imagePath);
            Assert.False(File.Exists(oldImagePath));
            Assert.True(File.Exists(Path.Combine(root, "wwwroot", "saints", "new-slug", "image.png")));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldThrowFormatException_WhenBase64ImageIsInvalid()
    {
        var root = CreateTempRoot();
        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<FormatException>(() =>
                service.SaveFilesAsync(
                    "saints",
                    "saint-test",
                    "markdown",
                    "data:image/png;base64,@@invalid@@"
                )
            );
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
