using FakeItEasy;
using Serilog;
using Shouldly;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;

namespace StlOrganizer.Library.Tests;

public class OperationSelectorTests
{
    private readonly IFileDecompressor fileDecompressor = A.Fake<IFileDecompressor>();
    private readonly IFolderCompressor folderCompressor = A.Fake<IFolderCompressor>();
    private readonly IImageOrganizer imageOrganizer = A.Fake<IImageOrganizer>();

    [Fact]
    public async Task ExecuteOperationAsync_WithFileDecompressor_CallsFileDecompressorAndReturnsMessage()
    {
        const string directoryPath = @"C:\test";
        var logger = A.Fake<ILogger>();
        var extractedFiles = new List<string> { "file1.txt", "file2.txt", "file3.txt" };
        
        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(Task.FromResult<IEnumerable<string>>(extractedFiles));
        
        var selector = new OperationSelector(fileDecompressor, folderCompressor, imageOrganizer, logger);

        var result = await selector.ExecuteOperationAsync(OperationType.FileDecompressor, directoryPath);

        result.ShouldBe("Successfully extracted 3 file(s).");
        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithFolderCompressor_CallsFolderCompressorAndReturnsMessage()
    {
        const string directoryPath = @"C:\test";
        const string outputPath = @"C:\test.zip";
        var logger = A.Fake<ILogger>();
        
        A.CallTo(() => folderCompressor.CompressFolder(directoryPath, null))
            .Returns(outputPath);
        
        var selector = new OperationSelector(fileDecompressor, folderCompressor, imageOrganizer, logger);

        var result = await selector.ExecuteOperationAsync(OperationType.FolderCompressor, directoryPath);

        result.ShouldBe($"Successfully created archive: {outputPath}");
        A.CallTo(() => folderCompressor.CompressFolder(directoryPath, null)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithImageOrganizer_CallsImageOrganizerAndReturnsMessage()
    {
        const string directoryPath = @"C:\test";
        const int copiedCount = 5;

        var logger = A.Fake<ILogger>();
        
        A.CallTo(() => imageOrganizer.OrganizeImagesAsync(directoryPath))
            .Returns(copiedCount);
        
        var selector = new OperationSelector(fileDecompressor, folderCompressor, imageOrganizer, logger);

        var result = await selector.ExecuteOperationAsync(OperationType.ImageOrganizer, directoryPath);

        result.ShouldBe("Successfully copied 5 image(s) to Images folder.");
        A.CallTo(() => imageOrganizer.OrganizeImagesAsync(directoryPath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithInvalidOperationType_ThrowsArgumentException()
    {
        const string directoryPath = @"C:\test";
        var logger = A.Fake<ILogger>();
        
        var selector = new OperationSelector(fileDecompressor, folderCompressor, imageOrganizer, logger);

        await Should.ThrowAsync<ArgumentException>(async () =>
            await selector.ExecuteOperationAsync((OperationType)999, directoryPath));
    }

    [Fact]
    public async Task ExecuteOperationAsync_FileDecompressor_LogsInformation()
    {
        const string directoryPath = @"C:\test";
        var logger = A.Fake<ILogger>();
        var extractedFiles = new List<string> { "file1.txt" };
        
        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(Task.FromResult<IEnumerable<string>>(extractedFiles));
        
        var selector = new OperationSelector(fileDecompressor, folderCompressor, imageOrganizer, logger);

        await selector.ExecuteOperationAsync(OperationType.FileDecompressor, directoryPath);

        A.CallTo(()=> logger.Information("FileDecompressor extracted {fileCount} files", 1)).MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteOperationAsync_FolderCompressor_LogsInformation()
    {
        const string directoryPath = @"C:\test";
        const string outputPath = @"C:\test.zip";
        var logger = A.Fake<ILogger>();
        
        A.CallTo(() => folderCompressor.CompressFolder(directoryPath, null))
            .Returns(outputPath);
        
        var selector = new OperationSelector(fileDecompressor, folderCompressor, imageOrganizer, logger);

        await selector.ExecuteOperationAsync(OperationType.FolderCompressor, directoryPath);

        A.CallTo(()=> logger.Information("FolderCompressor created archive at {OutputPath}", outputPath)).MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteOperationAsync_ImageOrganizer_LogsInformation()
    {
        const string directoryPath = @"C:\test";
        const int copiedCount = 5;
        var logger = A.Fake<ILogger>();
        
        A.CallTo(() => imageOrganizer.OrganizeImagesAsync(directoryPath))
            .Returns(copiedCount);
        
        var selector = new OperationSelector(fileDecompressor, folderCompressor, imageOrganizer, logger);

        await selector.ExecuteOperationAsync(OperationType.ImageOrganizer, directoryPath);
        
        A.CallTo(()=> logger.Information("ImageOrganizer copied {CopiedCount} images", copiedCount)).MustHaveHappened();
    }
}
