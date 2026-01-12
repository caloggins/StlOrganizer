using Serilog;
using StlOrganizer.Library.Compression;
using StlOrganizer.Library.Decompression;
using StlOrganizer.Library.ImageProcessing;

namespace StlOrganizer.Library;

public class OperationSelector(
    IDecompressionWorkflow decompressionWorkflow,
    IFolderCompressor folderCompressor,
    IImageOrganizer imageOrganizer,
    ILogger logger) : IOperationSelector
{
    public async Task<string> ExecuteOperationAsync(FileOperation operationType,
        string directoryPath,
        CancellationToken ct)
    {
        return operationType switch
        {
            _ when operationType == FileOperation.DecompressFiles => await ExecuteFileDecompressorAsync(directoryPath),
            _ when operationType == FileOperation.CompressFolder => await ExecuteFolderCompressorAsync(directoryPath),
            _ when operationType == FileOperation.ExtractImages => await ExecuteImageOrganizerAsync(directoryPath),
            _ => throw new ArgumentException($"Unknown operation type: {operationType.Name}")
        };
    }

    private async Task<string> ExecuteFileDecompressorAsync(string directoryPath)
    {
        var extractedFiles = await decompressionWorkflow.ExecuteAsync(directoryPath, false);
        var fileCount = extractedFiles.Count();
        logger.Information("DecompressionWorkflow extracted {fileCount} files", fileCount);
        return $"Successfully extracted {fileCount} file(s) and flattened folders.";
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