using Shouldly;

namespace StlOrganizer.Library.Tests;

public class FileOperationTests
{
    [Fact]
    public void DecompressFiles_WhenCreated_ShouldHaveValue()
    {
        FileOperation.DecompressFiles.Id.ShouldBe(1);
        FileOperation.DecompressFiles.Name.ShouldBe("Decompress files");
    }

    [Fact]
    public void CompressFolder_WhenCreated_ShouldHaveValue()
    {
        FileOperation.CompressFolder.Id.ShouldBe(2);
        FileOperation.CompressFolder.Name.ShouldBe("Compress folder");
    }

    [Fact]
    public void ExtractImages_WhenCreated_ShouldHaveValue()
    {
        FileOperation.ExtractImages.Id.ShouldBe(3);
        FileOperation.ExtractImages.Name.ShouldBe("Extract images");
    }
}