using FakeItEasy;
using Serilog;
using Shouldly;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Tests.Decompression;

public class FileDecompressorTests
{
    private readonly IFileSystem fileSystem;
    private readonly ILogger logger;
    private readonly FileDecompressor decompressor;

    public FileDecompressorTests()
    {
        fileSystem = A.Fake<IFileSystem>();
        logger = A.Fake<ILogger>();
        decompressor = new FileDecompressor(fileSystem, logger);
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WhenDirectoryDoesNotExist_ThrowsDirectoryNotFoundException()
    {
        const string directoryPath = "C:\\NonExistent";
        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(false);

        await Should.ThrowAsync<DirectoryNotFoundException>(
            async () => await decompressor.ScanAndDecompressAsync(directoryPath));
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WhenDirectoryExists_ChecksDirectoryExists()
    {
        const string directoryPath = "C:\\TestDir";
        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns(Array.Empty<string>());
        await decompressor.ScanAndDecompressAsync(directoryPath);

        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WhenNoCompressedFiles_ReturnsEmptyList()
    {
        const string directoryPath = "C:\\TestDir";
        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns(["file1.txt", "file2.doc"]);
        var result = await decompressor.ScanAndDecompressAsync(directoryPath);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WhenZipFileExists_ScansRecursively()
    {
        const string directoryPath = "C:\\TestDir";
        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns(["C:\\TestDir\\archive.zip"]);
        await decompressor.ScanAndDecompressAsync(directoryPath);

        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WithZipFile_FindsZipExtension()
    {
        const string directoryPath = "C:\\TestDir";
        const string zipFile = "C:\\TestDir\\archive.zip";

        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns([zipFile, "other.txt"]);
        A.CallTo(() => fileSystem.GetDirectoryName(zipFile)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(zipFile)).Returns("archive");
        A.CallTo(() => fileSystem.GetExtension(zipFile)).Returns(".zip");
        await decompressor.ScanAndDecompressAsync(directoryPath);

        A.CallTo(() => fileSystem.CreateDirectory(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WithGzipFile_FindsGzExtension()
    {
        const string directoryPath = "C:\\TestDir";
        const string gzipFile = "C:\\TestDir\\file.gz";

        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns([gzipFile]);
        A.CallTo(() => fileSystem.GetDirectoryName(gzipFile)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(gzipFile)).Returns("file");
        A.CallTo(() => fileSystem.GetExtension(gzipFile)).Returns(".gz");
        await decompressor.ScanAndDecompressAsync(directoryPath);

        A.CallTo(() => fileSystem.CreateDirectory(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WithTarFile_FindsTarExtension()
    {
        const string directoryPath = "C:\\TestDir";
        const string tarFile = "C:\\TestDir\\archive.tar";

        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns([tarFile]);
        A.CallTo(() => fileSystem.GetDirectoryName(tarFile)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(tarFile)).Returns("archive");
        A.CallTo(() => fileSystem.GetExtension(tarFile)).Returns(".tar");
        await decompressor.ScanAndDecompressAsync(directoryPath);

        A.CallTo(() => fileSystem.CreateDirectory(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WithTarGzFile_FindsTarGzExtension()
    {
        const string directoryPath = "C:\\TestDir";
        const string tarGzFile = "C:\\TestDir\\archive.tar.gz";

        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns([tarGzFile]);
        A.CallTo(() => fileSystem.GetDirectoryName(tarGzFile)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(tarGzFile)).Returns("archive.tar");
        A.CallTo(() => fileSystem.GetExtension(tarGzFile)).Returns(".gz");
        await decompressor.ScanAndDecompressAsync(directoryPath);

        A.CallTo(() => fileSystem.CreateDirectory(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public async Task ScanAndDecompressAsync_WithMultipleCompressedFiles_ProcessesAll()
    {
        const string directoryPath = "C:\\TestDir";
        const string zipFile = "C:\\TestDir\\archive.zip";
        const string gzFile = "C:\\TestDir\\file.gz";

        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns([zipFile, gzFile]);
        A.CallTo(() => fileSystem.GetDirectoryName(A<string>._)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(zipFile)).Returns("archive");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(gzFile)).Returns("file");
        A.CallTo(() => fileSystem.GetExtension(zipFile)).Returns(".zip");
        A.CallTo(() => fileSystem.GetExtension(gzFile)).Returns(".gz");
        await decompressor.ScanAndDecompressAsync(directoryPath);

        A.CallTo(() => fileSystem.CreateDirectory(A<string>._))
            .MustHaveHappened(2, Times.Exactly);
    }

    [Fact]
    public async Task ScanAndDecompressAsync_CreatesOutputDirectory()
    {
        const string directoryPath = "C:\\TestDir";
        const string zipFile = "C:\\TestDir\\archive.zip";

        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns([zipFile]);
        A.CallTo(() => fileSystem.GetDirectoryName(zipFile)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(zipFile)).Returns("archive");
        A.CallTo(() => fileSystem.GetExtension(zipFile)).Returns(".zip");
        await decompressor.ScanAndDecompressAsync(directoryPath);

        A.CallTo(() => fileSystem.CreateDirectory("C:\\TestDir\\archive"))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ScanAndDecompressAsync_IsCaseInsensitive()
    {
        const string directoryPath = "C:\\TestDir";
        const string zipFile = "C:\\TestDir\\ARCHIVE.ZIP";

        A.CallTo(() => fileSystem.DirectoryExists(directoryPath)).Returns(true);
        A.CallTo(() => fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories))
            .Returns([zipFile]);
        A.CallTo(() => fileSystem.GetDirectoryName(zipFile)).Returns("C:\\TestDir");
        A.CallTo(() => fileSystem.GetFileNameWithoutExtension(zipFile)).Returns("ARCHIVE");
        A.CallTo(() => fileSystem.GetExtension(zipFile)).Returns(".ZIP");
        await decompressor.ScanAndDecompressAsync(directoryPath);

        A.CallTo(() => fileSystem.CreateDirectory(A<string>._)).MustHaveHappened();
    }
}
