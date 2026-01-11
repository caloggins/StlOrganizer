using Serilog;
using StlOrganizer.Library.SystemFileAdapters;

namespace StlOrganizer.Library.ImageProcessing;

public class ImageOrganizer(IFileSystem fileSystem, IFileOperations fileOperations, ILogger logger) : IImageOrganizer
{
    private static readonly string[] ImageExtensions =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".svg"
    };

    private const string ImagesFolderName = "Images";

    public async Task<int> OrganizeImagesAsync(string rootPath)
    {
        if (!fileSystem.DirectoryExists(rootPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");
        }

        var imagesFolder = Path.Combine(rootPath, ImagesFolderName);
        fileSystem.CreateDirectory(imagesFolder);

        var copiedCount = 0;

        await Task.Run(() =>
        {
            copiedCount = ScanAndCopyImages(rootPath, imagesFolder);
        });

        logger.Information("Organized {CopiedCount} image(s) into {ImagesFolder}", copiedCount, imagesFolder);
        return copiedCount;
    }

    private int ScanAndCopyImages(string currentPath, string imagesFolder)
    {
        var copiedCount = 0;

        if (Path.GetFullPath(currentPath) == Path.GetFullPath(imagesFolder))
            return copiedCount;

        var files = fileSystem.GetFiles(currentPath, "*.*", SearchOption.TopDirectoryOnly);
        
        foreach (var file in files)
        {
            if (IsImageFile(file))
            {
                try
                {
                    CopyImageToFolder(file, imagesFolder);
                    copiedCount++;
                    logger.Debug("Copied image {FileName} to Images folder", fileOperations.GetFileName(file));
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Failed to copy image {FileName}", file);
                }
            }
        }

        var subdirectories = fileSystem.GetDirectories(currentPath);
        foreach (var subdirectory in subdirectories)
        {
            copiedCount += ScanAndCopyImages(subdirectory, imagesFolder);
        }

        return copiedCount;
    }

    private bool IsImageFile(string filePath)
    {
        var extension = fileSystem.GetExtension(filePath).ToLowerInvariant();
        return ImageExtensions.Contains(extension);
    }

    private void CopyImageToFolder(string sourceFile, string imagesFolder)
    {
        var fileName = fileOperations.GetFileName(sourceFile);
        var destinationPath = Path.Combine(imagesFolder, fileName);

        if (fileOperations.FileExists(destinationPath))
        {
            destinationPath = GenerateUniqueFileName(imagesFolder, fileName);
        }

        fileOperations.CopyFile(sourceFile, destinationPath, false);
    }

    private string GenerateUniqueFileName(string directory, string fileName)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var counter = 1;

        string newFileName;
        do
        {
            newFileName = Path.Combine(directory, $"{fileNameWithoutExtension}_{counter}{extension}");
            counter++;
        } while (fileOperations.FileExists(newFileName));

        return newFileName;
    }
}
