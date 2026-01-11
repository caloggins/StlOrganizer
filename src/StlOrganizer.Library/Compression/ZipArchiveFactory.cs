using System.IO.Compression;

namespace StlOrganizer.Library.Compression;

public class ZipArchiveFactory : IZipArchiveFactory
{
    public IZipArchive Open(string archiveFileName, ZipArchiveMode mode)
    {
        var zipArchive = ZipFile.Open(archiveFileName, mode);
        return new ZipArchiveAdapter(zipArchive);
    }
}
