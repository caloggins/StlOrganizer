using FakeItEasy;
using Serilog;
using Shouldly;
using StlOrganizer.Library.Decompression;

namespace StlOrganizer.Library.Tests.Decompression;

public class DecompressionWorkflowTests
{
    private readonly IFileDecompressor fileDecompressor;
    private readonly IFolderFlattener folderFlattener;
    private readonly ILogger logger;
    private readonly DecompressionWorkflow workflow;

    public DecompressionWorkflowTests()
    {
        fileDecompressor = A.Fake<IFileDecompressor>();
        folderFlattener = A.Fake<IFolderFlattener>();
        logger = A.Fake<ILogger>();
        workflow = new DecompressionWorkflow(fileDecompressor, folderFlattener, logger);
    }

    [Fact]
    public async Task ExecuteAsync_CallsFileDecompressorFirst()
    {
        const string directoryPath = @"C:\TestDir";
        var extractedFiles = new List<string> { "file1.txt", "file2.txt" };

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(extractedFiles);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_CallsFolderFlattenerAfterDecompression()
    {
        const string directoryPath = @"C:\TestDir";
        var extractedFiles = new List<string> { "file1.txt" };

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(extractedFiles);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => folderFlattener.RemoveNestedFolders(directoryPath))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_CallsOperationsInCorrectOrder()
    {
        const string directoryPath = @"C:\TestDir";
        var extractedFiles = new List<string> { "file1.txt" };
        var callOrder = new List<string>();

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Invokes(() => callOrder.Add("decompress"))
            .Returns(extractedFiles);

        A.CallTo(() => folderFlattener.RemoveNestedFolders(directoryPath))
            .Invokes(() => callOrder.Add("flatten"));

        await workflow.ExecuteAsync(directoryPath);

        callOrder.ShouldBe(new[] { "decompress", "flatten" });
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExtractedFilesFromDecompressor()
    {
        const string directoryPath = @"C:\TestDir";
        var extractedFiles = new List<string> { "file1.txt", "file2.txt", "file3.txt" };

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(extractedFiles);

        var result = await workflow.ExecuteAsync(directoryPath);

        result.ShouldBe(extractedFiles);
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectDirectoryPathToDecompressor()
    {
        const string directoryPath = @"C:\MyDirectory";
        var extractedFiles = new List<string> { "file1.txt" };

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(extractedFiles);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_PassesCorrectDirectoryPathToFlattener()
    {
        const string directoryPath = @"C:\MyDirectory";
        var extractedFiles = new List<string> { "file1.txt" };

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(extractedFiles);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => folderFlattener.RemoveNestedFolders(directoryPath))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFilesExtracted_StillCallsFlattener()
    {
        const string directoryPath = @"C:\TestDir";
        var emptyList = new List<string>();

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(emptyList);

        var result = await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => folderFlattener.RemoveNestedFolders(directoryPath))
            .MustHaveHappenedOnceExactly();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_LogsStartOfWorkflow()
    {
        const string directoryPath = @"C:\TestDir";
        var extractedFiles = new List<string> { "file1.txt" };

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(extractedFiles);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => logger.Information(
                A<string>.That.Contains("Starting decompression workflow"),
                directoryPath))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_LogsDecompressionCompletion()
    {
        const string directoryPath = @"C:\TestDir";
        var extractedFiles = new List<string> { "file1.txt", "file2.txt" };

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(extractedFiles);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => logger.Information(
                A<string>.That.Contains("Decompressed"),
                2))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_LogsFlattenerCompletion()
    {
        const string directoryPath = @"C:\TestDir";
        var extractedFiles = new List<string> { "file1.txt" };

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Returns(extractedFiles);

        await workflow.ExecuteAsync(directoryPath);

        A.CallTo(() => logger.Information(
                A<string>.That.Contains("Completed folder flattening"),
                directoryPath))
            .MustHaveHappened();
    }

    [Fact]
    public async Task ExecuteAsync_WhenDecompressorThrows_DoesNotCallFlattener()
    {
        const string directoryPath = @"C:\TestDir";

        A.CallTo(() => fileDecompressor.ScanAndDecompressAsync(directoryPath))
            .Throws<DirectoryNotFoundException>();

        await Should.ThrowAsync<DirectoryNotFoundException>(
            async () => await workflow.ExecuteAsync(directoryPath));

        A.CallTo(() => folderFlattener.RemoveNestedFolders(A<string>._))
            .MustNotHaveHappened();
    }
}
