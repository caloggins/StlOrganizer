using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Decompression;

public class FolderScanner(
    IFileSystem fileSystem,
    IDecompressor decompressor) : IFolderScanner
{
    public async Task ScanAndDecompressAsync(
        string folder,
        CancellationToken cancellationToken = default)
    {
        var files = fileSystem.GetFiles(folder, "*.zip", SearchOption.AllDirectories)
            .ToList();
        
        if (files.Count == 0)
            throw new NoArchivesFoundException();
        
        foreach (var file in files)
        {
            await decompressor.DecompressAsync(file, folder, cancellationToken);
        }
    }
}
