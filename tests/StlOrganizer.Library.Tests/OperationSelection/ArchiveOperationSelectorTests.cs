using FakeItEasy;
using Serilog;
using Shouldly;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;
using StlOrganizer.Library.OperationSelection;

namespace StlOrganizer.Library.Tests.OperationSelection;

public class ArchiveOperationSelectorTests
{
    private readonly IDecompressionWorkflow decompressionWorkflow = A.Fake<IDecompressionWorkflow>();
    private readonly IFolderCompressor folderCompressor = A.Fake<IFolderCompressor>();
    private readonly IImageOrganizer imageOrganizer = A.Fake<IImageOrganizer>();

    [Fact]
    public async Task ExecuteOperationAsync_WithFolderCompressor_CallsFolderCompressorAndReturnsMessage()
    {
        const string directoryPath = @"C:\test";
        const string outputPath = @"C:\test.zip";
        var logger = A.Fake<ILogger>();

        A.CallTo(() => folderCompressor.CompressFolder(directoryPath, null))
            .Returns(outputPath);

        var selector = new ArchiveOperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        var result =
            await selector.ExecuteOperationAsync(
                ArchiveOperation.CompressFolder, 
                directoryPath, 
                new Progress<OrganizerProgress>(),
                CancellationToken.None);

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

        var selector = new ArchiveOperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        var result =
            await selector.ExecuteOperationAsync(
                ArchiveOperation.ExtractImages,
                directoryPath,
                new Progress<OrganizerProgress>(), 
                CancellationToken.None);

        result.ShouldBe("Successfully copied 5 image(s) to Images folder.");
        A.CallTo(() => imageOrganizer.OrganizeImagesAsync(directoryPath)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task ExecuteOperationAsync_FolderCompressor_LogsInformation()
    {
        const string directoryPath = @"C:\test";
        const string outputPath = @"C:\test.zip";
        var logger = A.Fake<ILogger>();

        A.CallTo(() => folderCompressor.CompressFolder(directoryPath, null))
            .Returns(outputPath);

        var selector = new ArchiveOperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        await selector.ExecuteOperationAsync(
            ArchiveOperation.CompressFolder, 
            directoryPath, 
            new Progress<OrganizerProgress>(),
            CancellationToken.None);

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

        var selector = new ArchiveOperationSelector(decompressionWorkflow, folderCompressor, imageOrganizer, logger);

        await selector.ExecuteOperationAsync(
            ArchiveOperation.ExtractImages, 
            directoryPath, 
            new Progress<OrganizerProgress>(), 
            CancellationToken.None);

        A.CallTo(() => logger.Information("ImageOrganizer copied {CopiedCount} images", copiedCount))
            .MustHaveHappened();
    }
}