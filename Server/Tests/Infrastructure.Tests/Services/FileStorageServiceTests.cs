using Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SkiaSharp;

namespace Infrastructure.Tests.Services;

public class FileStorageServiceTests
{
    private const int MaxImageSize = 5 * 1024 * 1024;

    private static FileStorageService CreateService(string contentRootPath)
    {
        var env = new Mock<IHostEnvironment>();
        env.SetupGet(x => x.ContentRootPath).Returns(contentRootPath);

        return new FileStorageService(
            env.Object,
            NullLogger<FileStorageService>.Instance
        );
    }

    private static string CreateTempRoot()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "sam-tests",
            Guid.NewGuid().ToString("N")
        );

        Directory.CreateDirectory(root);
        return root;
    }

    private static byte[] CreatePngImage()
    {
        using var bitmap = new SKBitmap(10, 10);
        bitmap.Erase(SKColors.Red);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    private static string CreateImageDataUri()
    {
        var base64 = Convert.ToBase64String(CreatePngImage());
        return $"data:image/png;base64,{base64}";
    }

    private static string GetWwwRoot(string root)
        => Path.Combine(root, "wwwroot");

    private static string GetSaintFolder(string root, string slug)
        => Path.Combine(GetWwwRoot(root), "saints", slug);

    private static void Cleanup(string root)
    {
        if (Directory.Exists(root))
            Directory.Delete(root, recursive: true);
    }

    // GenerateSlug

    [Theory]
    [InlineData("Saint Francis", "saint-francis")]
    [InlineData("SAINT FRANCIS", "saint-francis")]
    [InlineData("Saint François 2026!", "saint-fran-ois-2026")]
    [InlineData("Saint   Francis", "saint-francis")]
    [InlineData("Saint-Francis", "saint-francis")]
    [InlineData("Saint.Francis", "saint-francis")]
    [InlineData("123 Saint 456", "123-saint-456")]
    [InlineData("  Saint Francis  ", "saint-francis")]
    [InlineData("!!!Saint!!!Francis!!!", "saint-francis")]
    [InlineData("", "")]
    [InlineData("!!!", "")]
    [InlineData("12345", "12345")]
    public void GenerateSlug_ShouldNormalizeText(
        string input,
        string expected)
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var result = service.GenerateSlug(input);

            Assert.Equal(expected, result);
        }
        finally
        {
            Cleanup(root);
        }
    }

    // DeleteFolderAsync

    [Fact]
    public async Task DeleteFolderAsync_ShouldDeleteExistingFolder()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);
            var folder = GetSaintFolder(root, "saint-francis");

            Directory.CreateDirectory(folder);

            await File.WriteAllTextAsync(
                Path.Combine(folder, "markdown.md"),
                "content"
            );

            await service.DeleteFolderAsync("saints", "saint-francis");

            Assert.False(Directory.Exists(folder));
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task DeleteFolderAsync_ShouldNotThrow_WhenFolderDoesNotExist()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await service.DeleteFolderAsync("saints", "missing");

            Assert.False(
                Directory.Exists(GetSaintFolder(root, "missing"))
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task DeleteFolderAsync_ShouldThrow_WhenFolderNameContainsPathTraversal()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.DeleteFolderAsync("../outside", "folder")
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task DeleteFolderAsync_ShouldThrow_WhenSlugContainsPathTraversal()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.DeleteFolderAsync("saints", "../outside")
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    // RenameFolderIfNeededAsync

    [Fact]
    public async Task RenameFolderIfNeededAsync_ShouldMoveFolder_WhenOldFolderExists()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var oldFolder = GetSaintFolder(root, "old-slug");
            var newFolder = GetSaintFolder(root, "new-slug");

            Directory.CreateDirectory(oldFolder);

            await File.WriteAllTextAsync(
                Path.Combine(oldFolder, "markdown.md"),
                "content"
            );

            await service.RenameFolderIfNeededAsync(
                "saints",
                "old-slug",
                "new-slug"
            );

            Assert.False(Directory.Exists(oldFolder));
            Assert.True(Directory.Exists(newFolder));

            Assert.Equal(
                "content",
                await File.ReadAllTextAsync(
                    Path.Combine(newFolder, "markdown.md")
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task RenameFolderIfNeededAsync_ShouldCreateNewFolder_WhenOldFolderMissing()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);
            var newFolder = GetSaintFolder(root, "new-slug");

            await service.RenameFolderIfNeededAsync(
                "saints",
                "missing-old",
                "new-slug"
            );

            Assert.True(Directory.Exists(newFolder));
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task RenameFolderIfNeededAsync_ShouldSkipRename_WhenSlugsAreEqual()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);
            var folder = GetSaintFolder(root, "saint-francis");

            Directory.CreateDirectory(folder);

            await service.RenameFolderIfNeededAsync(
                "saints",
                "saint-francis",
                "saint-francis"
            );

            Assert.True(Directory.Exists(folder));
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task RenameFolderIfNeededAsync_ShouldSkipRename_WhenSlugsDifferOnlyByCase()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);
            var folder = GetSaintFolder(root, "saint-francis");

            Directory.CreateDirectory(folder);

            await service.RenameFolderIfNeededAsync(
                "saints",
                "saint-francis",
                "SAINT-FRANCIS"
            );

            Assert.True(Directory.Exists(folder));
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task RenameFolderIfNeededAsync_ShouldThrow_WhenDestinationAlreadyExists()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var oldFolder = GetSaintFolder(root, "old-slug");
            var newFolder = GetSaintFolder(root, "new-slug");

            Directory.CreateDirectory(oldFolder);
            Directory.CreateDirectory(newFolder);

            await Assert.ThrowsAsync<IOException>(
                () => service.RenameFolderIfNeededAsync(
                    "saints",
                    "old-slug",
                    "new-slug"
                )
            );

            Assert.True(Directory.Exists(oldFolder));
            Assert.True(Directory.Exists(newFolder));
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task RenameFolderIfNeededAsync_ShouldThrow_WhenOldSlugContainsPathTraversal()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.RenameFolderIfNeededAsync(
                    "saints",
                    "../outside",
                    "new-slug"
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task RenameFolderIfNeededAsync_ShouldThrow_WhenNewSlugContainsPathTraversal()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.RenameFolderIfNeededAsync(
                    "saints",
                    "old-slug",
                    "../outside"
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    // SaveFilesAsync - Markdown

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

            Assert.Equal(
                "/saints/saint-francis/markdown.md",
                markdownPath
            );

            Assert.Null(imagePath);

            var fullPath = Path.Combine(
                GetSaintFolder(root, "saint-francis"),
                "markdown.md"
            );

            Assert.True(File.Exists(fullPath));

            Assert.Equal(
                "# Saint Francis",
                await File.ReadAllTextAsync(fullPath)
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldOverwriteExistingMarkdown()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);
            var folder = GetSaintFolder(root, "saint-francis");

            Directory.CreateDirectory(folder);

            var markdownFile = Path.Combine(folder, "markdown.md");

            await File.WriteAllTextAsync(markdownFile, "old content");

            await service.SaveFilesAsync(
                "saints",
                "saint-francis",
                "new content",
                null
            );

            Assert.Equal(
                "new content",
                await File.ReadAllTextAsync(markdownFile)
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldCreateDirectory_WhenDestinationDoesNotExist()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);
            var folder = GetSaintFolder(root, "new-saint");

            Assert.False(Directory.Exists(folder));

            await service.SaveFilesAsync(
                "saints",
                "new-saint",
                "markdown",
                null
            );

            Assert.True(Directory.Exists(folder));
            Assert.True(File.Exists(Path.Combine(folder, "markdown.md")));
        }
        finally
        {
            Cleanup(root);
        }
    }

    // SaveFilesAsync - Base64 image

    [Fact]
    public async Task SaveFilesAsync_ShouldConvertValidPngToWebp()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);
            var image = CreateImageDataUri();

            var (markdownPath, imagePath) = await service.SaveFilesAsync(
                "saints",
                "saint-francis",
                "markdown",
                image
            );

            Assert.Equal(
                "/saints/saint-francis/markdown.md",
                markdownPath
            );

            Assert.Equal(
                "/saints/saint-francis/image.webp",
                imagePath
            );

            var fullImagePath = Path.Combine(
                GetSaintFolder(root, "saint-francis"),
                "image.webp"
            );

            Assert.True(File.Exists(fullImagePath));

            using var decodedImage = SKBitmap.Decode(fullImagePath);

            Assert.NotNull(decodedImage);
            Assert.Equal(10, decodedImage.Width);
            Assert.Equal(10, decodedImage.Height);
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldThrowFormatException_WhenBase64ImageIsInvalid()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<FormatException>(
                () => service.SaveFilesAsync(
                    "saints",
                    "saint-test",
                    "markdown",
                    "data:image/png;base64,@@invalid@@"
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldThrowInvalidDataException_WhenImageCannotBeDecoded()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);
            var invalidImage = Convert.ToBase64String(
                new byte[] { 1, 2, 3, 4, 5 }
            );

            await Assert.ThrowsAsync<InvalidDataException>(
                () => service.SaveFilesAsync(
                    "saints",
                    "saint-test",
                    "markdown",
                    $"data:image/png;base64,{invalidImage}"
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldThrowInvalidDataException_WhenImageExceedsMaximumSize()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var oversizedBase64 = new string(
                'A',
                (MaxImageSize * 4 / 3) + 5
            );

            await Assert.ThrowsAsync<InvalidDataException>(
                () => service.SaveFilesAsync(
                    "saints",
                    "saint-test",
                    "markdown",
                    $"data:image/png;base64,{oversizedBase64}"
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldDeleteOldImage_WhenSavingNewBase64Image()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var oldFolder = GetSaintFolder(root, "old-slug");
            Directory.CreateDirectory(oldFolder);

            var oldImage = Path.Combine(oldFolder, "image.png");

            await File.WriteAllBytesAsync(
                oldImage,
                CreatePngImage()
            );

            var (_, imagePath) = await service.SaveFilesAsync(
                "saints",
                "new-slug",
                "markdown",
                CreateImageDataUri(),
                "/saints/old-slug/image.png"
            );

            Assert.False(File.Exists(oldImage));

            Assert.Equal(
                "/saints/new-slug/image.webp",
                imagePath
            );

            Assert.True(
                File.Exists(
                    Path.Combine(
                        GetSaintFolder(root, "new-slug"),
                        "image.webp"
                    )
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldReturnNullImagePath_WhenDataUriDoesNotMatchExpectedFormat()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var (_, imagePath) = await service.SaveFilesAsync(
                "saints",
                "saint-test",
                "markdown",
                "data:image/png"
            );

            Assert.Null(imagePath);
        }
        finally
        {
            Cleanup(root);
        }
    }

    // SaveFilesAsync - Existing image copy

    [Fact]
    public async Task SaveFilesAsync_ShouldCopyExistingImage_WhenImagePathExists()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var sourceFolder = GetSaintFolder(root, "old-slug");
            Directory.CreateDirectory(sourceFolder);

            var sourceImage = Path.Combine(sourceFolder, "image.png");

            var imageBytes = CreatePngImage();
            await File.WriteAllBytesAsync(sourceImage, imageBytes);

            var (_, imagePath) = await service.SaveFilesAsync(
                "saints",
                "new-slug",
                "markdown",
                "/saints/old-slug/image.png"
            );

            var destinationImage = Path.Combine(
                GetSaintFolder(root, "new-slug"),
                "image.png"
            );

            Assert.Equal(
                "/saints/new-slug/image.png",
                imagePath
            );

            Assert.True(File.Exists(sourceImage));
            Assert.True(File.Exists(destinationImage));

            Assert.Equal(
                imageBytes,
                await File.ReadAllBytesAsync(destinationImage)
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldReturnNullImagePath_WhenExistingImageDoesNotExist()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var (_, imagePath) = await service.SaveFilesAsync(
                "saints",
                "new-slug",
                "markdown",
                "/saints/missing/image.png"
            );

            Assert.Null(imagePath);
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldNotCopyImage_WhenSourceAndDestinationAreSame()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var folder = GetSaintFolder(root, "saint-francis");
            Directory.CreateDirectory(folder);

            var imageFile = Path.Combine(folder, "image.png");
            await File.WriteAllBytesAsync(imageFile, CreatePngImage());

            var (_, imagePath) = await service.SaveFilesAsync(
                "saints",
                "saint-francis",
                "markdown",
                "/saints/saint-francis/image.png"
            );

            Assert.Equal(
                "/saints/saint-francis/image.png",
                imagePath
            );

            Assert.True(File.Exists(imageFile));
        }
        finally
        {
            Cleanup(root);
        }
    }

    // SaveFilesAsync - Existing image move

    [Fact]
    public async Task SaveFilesAsync_ShouldMoveExistingImage_WhenExistingImagePathProvided()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var oldFolder = GetSaintFolder(root, "old-slug");
            Directory.CreateDirectory(oldFolder);

            var oldImage = Path.Combine(oldFolder, "image.png");
            var imageBytes = CreatePngImage();

            await File.WriteAllBytesAsync(oldImage, imageBytes);

            var (markdownPath, imagePath) = await service.SaveFilesAsync(
                "saints",
                "new-slug",
                "updated markdown",
                null,
                "/saints/old-slug/image.png"
            );

            var newImage = Path.Combine(
                GetSaintFolder(root, "new-slug"),
                "image.png"
            );

            Assert.Equal(
                "/saints/new-slug/markdown.md",
                markdownPath
            );

            Assert.Equal(
                "/saints/new-slug/image.png",
                imagePath
            );

            Assert.False(File.Exists(oldImage));
            Assert.True(File.Exists(newImage));

            Assert.Equal(
                imageBytes,
                await File.ReadAllBytesAsync(newImage)
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldReturnNullImagePath_WhenExistingImageToMoveDoesNotExist()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var (_, imagePath) = await service.SaveFilesAsync(
                "saints",
                "new-slug",
                "markdown",
                null,
                "/saints/missing/image.png"
            );

            Assert.Null(imagePath);
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldNotMoveImage_WhenSourceAndDestinationAreSame()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var folder = GetSaintFolder(root, "saint-francis");
            Directory.CreateDirectory(folder);

            var imageFile = Path.Combine(folder, "image.png");
            await File.WriteAllBytesAsync(imageFile, CreatePngImage());

            var (_, imagePath) = await service.SaveFilesAsync(
                "saints",
                "saint-francis",
                "markdown",
                null,
                "/saints/saint-francis/image.png"
            );

            Assert.Equal(
                "/saints/saint-francis/image.png",
                imagePath
            );

            Assert.True(File.Exists(imageFile));
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldReturnNullImagePath_WhenImageAndExistingImageAreMissing()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            var (_, imagePath) = await service.SaveFilesAsync(
                "saints",
                "saint-francis",
                "markdown",
                null,
                null
            );

            Assert.Null(imagePath);
        }
        finally
        {
            Cleanup(root);
        }
    }

    // SaveFilesAsync - Path security

    [Fact]
    public async Task SaveFilesAsync_ShouldThrow_WhenFolderNameContainsPathTraversal()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.SaveFilesAsync(
                    "../outside",
                    "saint-test",
                    "markdown",
                    null
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldThrow_WhenSlugContainsPathTraversal()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.SaveFilesAsync(
                    "saints",
                    "../outside",
                    "markdown",
                    null
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldThrow_WhenExistingImagePathContainsPathTraversal()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.SaveFilesAsync(
                    "saints",
                    "saint-test",
                    "markdown",
                    null,
                    "/../outside/image.png"
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }

    [Fact]
    public async Task SaveFilesAsync_ShouldThrow_WhenImagePathContainsPathTraversal()
    {
        var root = CreateTempRoot();

        try
        {
            var service = CreateService(root);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => service.SaveFilesAsync(
                    "saints",
                    "saint-test",
                    "markdown",
                    "/../outside/image.png"
                )
            );
        }
        finally
        {
            Cleanup(root);
        }
    }
}