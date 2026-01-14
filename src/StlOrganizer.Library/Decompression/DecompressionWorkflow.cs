using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Decompression;

public class DecompressionWorkflow(
    IFolderScanner folderScanner,
    IFolderFlattener folderFlattener,
    IFileOperations fileOperations) : IDecompressionWorkflow
{
    public async Task Execute(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDirectoryIsMissing(directoryPath);
        
        await folderScanner.ScanAndDecompressAsync(directoryPath, cancellationToken);
        
        await folderFlattener.RemoveNestedFolders(directoryPath, cancellationToken);
    }

    private void ThrowIfDirectoryIsMissing(string path)
    {
        if (!fileOperations.DirectoryExists(path))
            throw new DirectoryNotFoundException(path);
    }
}
