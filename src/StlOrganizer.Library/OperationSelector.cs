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
        CancellationToken cancellationToken)
    {
        return operationType switch
        {
            _ when operationType == FileOperation.DecompressFiles => await ExecuteFileDecompressorAsync(directoryPath, cancellationToken),
            _ when operationType == FileOperation.CompressFolder => await ExecuteFolderCompressorAsync(directoryPath, cancellationToken),
            _ when operationType == FileOperation.ExtractImages => await ExecuteImageOrganizerAsync(directoryPath, cancellationToken),
            _ => throw new ArgumentException($"Unknown operation type: {operationType.Name}")
        };
    }

    private async Task<string> ExecuteFileDecompressorAsync(string directoryPath, CancellationToken cancellationToken)
    {
        await decompressionWorkflow.Execute(directoryPath, cancellationToken);
        var fileCount = 0;
        logger.Information("DecompressionWorkflow extracted {fileCount} files", fileCount);
        return $"Successfully extracted {fileCount} file(s) and flattened folders.";
    }

    private Task<string> ExecuteFolderCompressorAsync(string directoryPath, CancellationToken cancellationToken)
    {
        var outputPath = folderCompressor.CompressFolder(directoryPath, cancellationToken: cancellationToken);
        logger.Information("FolderCompressor created archive at {OutputPath}", outputPath);
        return Task.FromResult($"Successfully created archive: {outputPath}");
    }

    private async Task<string> ExecuteImageOrganizerAsync(string directoryPath, CancellationToken cancellationToken)
    {
        var copiedCount = await imageOrganizer.OrganizeImagesAsync(directoryPath, cancellationToken);
        logger.Information("ImageOrganizer copied {CopiedCount} images", copiedCount);
        return $"Successfully copied {copiedCount} image(s) to Images folder.";
    }
}