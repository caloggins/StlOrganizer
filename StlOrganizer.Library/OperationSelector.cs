using Serilog;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;

namespace StlOrganizer.Library;

public class OperationSelector(
    IFileDecompressor fileDecompressor,
    IFolderCompressor folderCompressor,
    IImageOrganizer imageOrganizer,
    ILogger logger) : IOperationSelector
{
    public async Task<string> ExecuteOperationAsync(OperationType operationType, string directoryPath)
    {
        return operationType switch
        {
            OperationType.FileDecompressor => await ExecuteFileDecompressorAsync(directoryPath),
            OperationType.FolderCompressor => await ExecuteFolderCompressorAsync(directoryPath),
            OperationType.ImageOrganizer => await ExecuteImageOrganizerAsync(directoryPath),
            _ => throw new ArgumentException($"Unknown operation type: {operationType}")
        };
    }

    private async Task<string> ExecuteFileDecompressorAsync(string directoryPath)
    {
        var extractedFiles = await fileDecompressor.ScanAndDecompressAsync(directoryPath);
        var fileCount = extractedFiles.Count();
        logger.Information("FileDecompressor extracted {fileCount} files", fileCount);
        return $"Successfully extracted {fileCount} file(s).";
    }

    private Task<string> ExecuteFolderCompressorAsync(string directoryPath)
    {
        var outputPath = folderCompressor.CompressFolder(directoryPath);
        logger.Information("FolderCompressor created archive at {OutputPath}", outputPath);
        return Task.FromResult($"Successfully created archive: {outputPath}");
    }

    private async Task<string> ExecuteImageOrganizerAsync(string directoryPath)
    {
        var copiedCount = await imageOrganizer.OrganizeImagesAsync(directoryPath);
        logger.Information("ImageOrganizer copied {CopiedCount} images", copiedCount);
        return $"Successfully copied {copiedCount} image(s) to Images folder.";
    }
}
