using System.IO.Compression;

namespace StlOrganizer.Library.Compression;

public class Compressor : ICompressor
{
    public async Task Compress(
        string source,
        string destination,
        CancellationToken cancellationToken = default)
    {
        await ZipFile.CreateFromDirectoryAsync(
            source,
            destination,
            CompressionLevel.Optimal,
            false,
            cancellationToken );
    }
}
