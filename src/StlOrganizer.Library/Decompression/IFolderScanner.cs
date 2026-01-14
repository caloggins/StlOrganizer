namespace StlOrganizer.Library.Decompression;

public interface IFolderScanner
{
    Task ScanAndDecompressAsync(
        string folder,
        CancellationToken cancellationToken = default);
}
