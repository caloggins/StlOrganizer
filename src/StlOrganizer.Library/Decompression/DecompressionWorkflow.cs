using StlOrganizer.Library.OperationSelection;
using StlOrganizer.Library.SystemAdapters.FileSystem;

namespace StlOrganizer.Library.Decompression;

public class DecompressionWorkflow(
    IFolderScanner folderScanner,
    IFolderFlattener folderFlattener,
    IFileOperations fileOperations) : IDecompressionWorkflow
{
    public async Task Execute(
        string directoryPath,
        IProgress<OrganizerProgress> progress,
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
