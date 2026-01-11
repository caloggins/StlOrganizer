using System.IO.Compression;

namespace StlOrganizer.Library.Compression;

public class ZipArchiveAdapter : IZipArchive
{
    private readonly ZipArchive zipArchive;

    public ZipArchiveAdapter(ZipArchive zipArchive)
    {
        this.zipArchive = zipArchive;
    }

    public void CreateEntryFromFile(string sourceFileName, string entryName, CompressionLevel compressionLevel)
    {
        zipArchive.CreateEntryFromFile(sourceFileName, entryName, compressionLevel);
    }

    public void Dispose()
    {
        zipArchive?.Dispose();
    }
}
