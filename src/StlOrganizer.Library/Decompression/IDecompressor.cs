namespace StlOrganizer.Library.Decompression;

public interface IDecompressor
{
    Task DecompressAsync(
        string file,
        string folder,
        CancellationToken cancellationToken);
}
