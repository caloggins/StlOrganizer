using FakeItEasy;
using Shouldly;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Tests.Decompression;

public class FolderScannerTests
{
    private readonly IFileSystem fileSystem = A.Fake<IFileSystem>();
    private readonly IDecompressor decompressor = A.Fake<IDecompressor>();
    private readonly FolderScanner sut;

    public FolderScannerTests()
    {
        sut = new FolderScanner(
            fileSystem,
            decompressor);
    }

    [Fact]
    public async Task DecompressArchives_WhenPathIsEmpty_ShouldThrowException()
    {
        A.CallTo(() => fileSystem.GetFiles(A<string>._, "*.zip", A<SearchOption>._))
            .Returns(new List<string>());

        await sut.ScanAndDecompressAsync(
                string.Empty,
                CancellationToken.None)
            .ShouldThrowAsync<NoArchivesFoundException>();
    }

    [Fact]
    public async Task DecompressArchives_WhenZipFilesAreFound_ShouldDecompressThem()
    {
        const string folder = @"C:\TestDir";
        const string file = "archive.zip";
        A.CallTo(() => fileSystem.GetFiles(folder, "*.zip", A<SearchOption>._))
            .Returns(new List<string> { file });

        await sut.ScanAndDecompressAsync(
            folder,
            CancellationToken.None);

        A.CallTo(() => decompressor.DecompressAsync(file, folder , A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }
}


