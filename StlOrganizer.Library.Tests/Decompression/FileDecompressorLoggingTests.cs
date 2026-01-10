using FakeItEasy;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;
using Shouldly;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Tests.Decompression;

public class FileDecompressorLoggingTests
{
    private readonly IFileSystem fileSystem;
    private readonly ILogger logger;
    private readonly FileDecompressor decompressor;

    public FileDecompressorLoggingTests()
    {
        fileSystem = A.Fake<IFileSystem>();
        logger = new LoggerConfiguration()
            .WriteTo.InMemory()
            .CreateLogger();
        decompressor = new FileDecompressor(fileSystem, logger);
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WhenDecompressionFails_LogsError()
    {
        const string directoryPath = "C:\\TestDir";
        const string zipFile = "C:\\TestDir\\corrupted.zip";
        
        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns(new[] { zipFile });
        A.CallTo(() => fileSystem.GetDirectoryName(zipFile)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(zipFile)).Returns("corrupted");
        A.CallTo(() => fileSystem.GetExtension(zipFile)).Returns(".zip");
        A.CallTo(() => fileSystem.CreateDirectory(A<string>._)).Throws(new IOException("Disk full"));

        await decompressor.ScanAndDecompressAsync(directoryPath);

        var logEvents = InMemorySink.Instance.LogEvents.ToList();
        logEvents.ShouldNotBeEmpty();
        logEvents.ShouldContain(e => e.Level == LogEventLevel.Error);
        logEvents.ShouldContain(e => e.MessageTemplate.Text.Contains("Failed to decompress"));
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WhenDecompressionFails_LogsFileName()
    {
        const string directoryPath = "C:\\TestDir";
        const string zipFile = "C:\\TestDir\\corrupted.zip";
        
        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns(new[] { zipFile });
        A.CallTo(() => fileSystem.GetDirectoryName(zipFile)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(zipFile)).Returns("corrupted");
        A.CallTo(() => fileSystem.GetExtension(zipFile)).Returns(".zip");
        A.CallTo(() => fileSystem.CreateDirectory(A<string>._)).Throws(new IOException("Disk full"));

        await decompressor.ScanAndDecompressAsync(directoryPath);

        var logEvents = InMemorySink.Instance.LogEvents.ToList();
        var errorLog = logEvents.FirstOrDefault(e => e.Level == LogEventLevel.Error);
        errorLog.ShouldNotBeNull();
        errorLog.Properties.ShouldContainKey("CompressedFile");
        errorLog.Properties["CompressedFile"].ToString().ShouldContain("corrupted.zip");
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WhenDecompressionFails_LogsException()
    {
        const string directoryPath = "C:\\TestDir";
        const string zipFile = "C:\\TestDir\\corrupted.zip";
        var expectedException = new IOException("Disk full");
        
        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns(new[] { zipFile });
        A.CallTo(() => fileSystem.GetDirectoryName(zipFile)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(zipFile)).Returns("corrupted");
        A.CallTo(() => fileSystem.GetExtension(zipFile)).Returns(".zip");
        A.CallTo(() => fileSystem.CreateDirectory(A<string>._)).Throws(expectedException);

        await decompressor.ScanAndDecompressAsync(directoryPath);

        var logEvents = InMemorySink.Instance.LogEvents.ToList();
        var errorLog = logEvents.FirstOrDefault(e => e.Level == LogEventLevel.Error);
        errorLog.ShouldNotBeNull();
        errorLog.Exception.ShouldBe(expectedException);
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WhenMultipleFilesFailDecompression_LogsAllErrors()
    {
        const string directoryPath = "C:\\TestDir";
        const string zipFile1 = "C:\\TestDir\\corrupted1.zip";
        const string zipFile2 = "C:\\TestDir\\corrupted2.zip";
        
        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns(new[] { zipFile1, zipFile2 });
        A.CallTo(() => fileSystem.GetDirectoryName(A<string>._)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(zipFile1)).Returns("corrupted1");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(zipFile2)).Returns("corrupted2");
        A.CallTo(() => fileSystem.GetExtension(A<string>._)).Returns(".zip");
        A.CallTo(() => fileSystem.CreateDirectory(A<string>._)).Throws(new IOException("Disk full"));

        await decompressor.ScanAndDecompressAsync(directoryPath);

        var logEvents = InMemorySink.Instance.LogEvents.ToList();
        var errorLogs = logEvents.Where(e => e.Level == LogEventLevel.Error).ToList();
        errorLogs.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WhenSuccessful_DoesNotLogErrors()
    {
        const string directoryPath = "C:\\TestDir";
        
        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns(Array.Empty<string>());

        await decompressor.ScanAndDecompressAsync(directoryPath);

        var logEvents = InMemorySink.Instance.LogEvents.ToList();
        var errorLogs = logEvents.Where(e => e.Level == LogEventLevel.Error).ToList();
        errorLogs.ShouldBeEmpty();
    }
}
