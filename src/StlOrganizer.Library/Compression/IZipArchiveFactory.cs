using System.IO.Compression;

namespace StlOrganizer.Library.Compression;

public interface IZipArchiveFactory
{
    IZipArchive Open(string archiveFileName, ZipArchiveMode mode);
}
