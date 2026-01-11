using System.IO.Compression;
using Serilog;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.Compression;

public class FolderCompressor(
    IFileSystem fileSystem,
    IFileOperations fileOperations,
    IZipArchiveFactory zipArchiveFactory,
    ILogger logger) : IFolderCompressor
{
    public string CompressFolder(string folderPath, string? outputPath = null)
    {
        if (!fileSystem.DirectoryExists(folderPath))
            throw new DirectoryNotFoundException($"Directory not found: {folderPath}");

        var folderName = fileSystem.GetDirectoryName(folderPath);
        var parentDirectory = fileSystem.GetParentDirectory(folderPath) ?? folderPath;

        outputPath ??= fileSystem.CombinePaths(parentDirectory, $"{folderName}.zip");

        if (fileOperations.FileExists(outputPath))
        {
            fileOperations.DeleteFile(outputPath);
            logger.Debug("Deleted existing archive {OutputPath}", outputPath);
        }

        using (var archive = zipArchiveFactory.Open(outputPath, ZipArchiveMode.Create))
        {
            AddDirectoryContentsToArchive(archive, folderPath, string.Empty);
        }

        logger.Information("Created archive {OutputPath} from folder {FolderPath}", outputPath, folderPath);
        return outputPath;
    }

    private void AddDirectoryContentsToArchive(IZipArchive archive, string directoryPath, string entryPrefix)
    {
        var files = fileSystem.GetFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
            try
            {
                var fileName = fileOperations.GetFileName(file);
                var entryName = string.IsNullOrEmpty(entryPrefix)
                    ? fileName
                    : fileSystem.CombinePaths(entryPrefix, fileName).Replace('\\', '/');

                archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
                logger.Debug("Added file {FileName} to archive as {EntryName}", fileName, entryName);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to add file {File} to archive", file);
            }
        }

        var subdirectories = fileSystem.GetDirectories(directoryPath);
        foreach (var subdirectory in subdirectories)
        {
            var subdirectoryName = fileSystem.GetDirectoryName(subdirectory);
            var newEntryPrefix = string.IsNullOrEmpty(entryPrefix)
                ? subdirectoryName
                : fileSystem.CombinePaths(entryPrefix, subdirectoryName).Replace('\\', '/');

            AddDirectoryContentsToArchive(archive, subdirectory, newEntryPrefix);
        }
    }
}
