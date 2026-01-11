using System.IO.Compression;

namespace StlOrganizer.Library.Compression;

public interface IZipArchive : IDisposable
{
    void CreateEntryFromFile(string sourceFileName, string entryName, CompressionLevel compressionLevel);
}
