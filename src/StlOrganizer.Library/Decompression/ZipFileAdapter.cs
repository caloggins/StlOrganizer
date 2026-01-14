using System.IO.Compression;

namespace StlOrganizer.Library.Decompression;

public class ZipFileAdapter : IDecompressor
{
    public async Task DecompressAsync(
        string file,
        string folder,
        CancellationToken cancellationToken)
    {
        await ZipFile.ExtractToDirectoryAsync(
            file, folder, cancellationToken);
    }
}