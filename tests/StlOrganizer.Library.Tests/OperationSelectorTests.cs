using FakeItEasy;
using Serilog;
using Shouldly;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;

namespace StlOrganizer.Library.Tests;

public class OperationSelectorTests
{
    private readonly IDecompressionWorkflow decompressionWorkflow = A.Fake<IDecompressionWorkflow>();
    private readonly IFolderCompressor folderCompressor = A.Fake<IFolderCompressor>();
    private readonly IImageOrganizer imageOrganizer = A.Fake<IImageOrganizer>();

    [Fact]
    public async Task ExecuteOperationAsync_WithFileDecompressor_CallsDecompressionWorkflowAndReturnsMessage()
    {
        const string directoryPath = @"C:\test";
        var logger = A.Fake<ILogger>();
        var extractedFiles = new List<string> { "file1.txt", "file2.txt", "file3.txt" };

        A.CallTo(() => decompressionWorkflow.ExecuteAsync(directoryPath, A<bool>._))
            .Returns(Task.FromResult<IEnumerable<string>>(extractedFiles));

        var selector = new OperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        var result =
            await selector.ExecuteOperationAsync(FileOperation.DecompressFiles, directoryPath, CancellationToken.None);

        result.ShouldBe("Successfully extracted 3 file(s) and flattened folders.");
        A.CallTo(() => decompressionWorkflow.ExecuteAsync(directoryPath, A<bool>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteOperationAsync_WithFolderCompressor_CallsFolderCompressorAndReturnsMessage()
    {
        const string directoryPath = @"C:\test";
        const string outputPath = @"C:\test.zip";
        var logger = A.Fake<ILogger>();

        A.CallTo(() => folderCompressor.CompressFolder(directoryPath, null))
            .Returns(outputPath);

        var selector = new OperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        var result =
            await selector.ExecuteOperationAsync(FileOperation.CompressFolder, directoryPath, CancellationToken.None);

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

        var selector = new OperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        var result =
            await selector.ExecuteOperationAsync(FileOperation.ExtractImages, directoryPath, CancellationToken.None);

        result.ShouldBe("Successfully copied 5 image(s) to Images folder.");
        A.CallTo(() => imageOrganizer.OrganizeImagesAsync(directoryPath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteOperationAsync_FileDecompressor_LogsInformation()
    {
        const string directoryPath = @"C:\test";
        var logger = A.Fake<ILogger>();
        var extractedFiles = new List<string> { "file1.txt" };

        A.CallTo(() => decompressionWorkflow.ExecuteAsync(directoryPath, A<bool>._))
            .Returns(Task.FromResult<IEnumerable<string>>(extractedFiles));

        var selector = new OperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        await selector.ExecuteOperationAsync(FileOperation.DecompressFiles, directoryPath, CancellationToken.None);

        A.CallTo(() => logger.Information("DecompressionWorkflow extracted {fileCount} files", 1)).MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteOperationAsync_FolderCompressor_LogsInformation()
    {
        const string directoryPath = @"C:\test";
        const string outputPath = @"C:\test.zip";
        var logger = A.Fake<ILogger>();

        A.CallTo(() => folderCompressor.CompressFolder(directoryPath, null))
            .Returns(outputPath);

        var selector = new OperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        await selector.ExecuteOperationAsync(FileOperation.CompressFolder, directoryPath, CancellationToken.None);

        A.CallTo(() => logger.Information("FolderCompressor created archive at {OutputPath}", outputPath))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteOperationAsync_ImageOrganizer_LogsInformation()
    {
        const string directoryPath = @"C:\test";
        const int copiedCount = 5;
        var logger = A.Fake<ILogger>();

        A.CallTo(() => imageOrganizer.OrganizeImagesAsync(directoryPath))
            .Returns(copiedCount);

        var selector = new OperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        await selector.ExecuteOperationAsync(FileOperation.ExtractImages, directoryPath, CancellationToken.None);

        A.CallTo(() => logger.Information("ImageOrganizer copied {CopiedCount} images", copiedCount))
            .MustHaveHappened();
    }
}