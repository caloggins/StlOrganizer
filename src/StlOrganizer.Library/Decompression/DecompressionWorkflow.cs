using Serilog;

namespace StlOrganizer.Library.Decompression;

public class DecompressionWorkflow(
    IFileDecompressor fileDecompressor,
    IFolderFlattener folderFlattener,
    ILogger logger) : IDecompressionWorkflow
{
    public async Task<IEnumerable<string>> ExecuteAsync(string directoryPath)
    {
        logger.Information("Starting decompression workflow for {DirectoryPath}", directoryPath);

        // Step 1: Decompress all files
        var extractedFiles = await fileDecompressor.ScanAndDecompressAsync(directoryPath);
        var fileCount = extractedFiles.Count();
        logger.Information("Decompressed {FileCount} files", fileCount);

        // Step 2: Flatten nested folders
        folderFlattener.RemoveNestedFolders(directoryPath);
        logger.Information("Completed folder flattening for {DirectoryPath}", directoryPath);

        return extractedFiles;
    }
}
