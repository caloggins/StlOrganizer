namespace StlOrganizer.Library.Compression;

public interface ICompressor
{
    Task Compress(
        string source,
        string destination,
        CancellationToken cancellationToken);
}
