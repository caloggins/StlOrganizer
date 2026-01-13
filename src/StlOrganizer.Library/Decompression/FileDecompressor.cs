using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Serilog;
using StlOrganizer.Library.SystemFileAdapters;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace StlOrganizer.Library.Decompression;

public class FileDecompressor(
    IFileSystem fileSystem,
    ILogger logger) : IFileDecompressor
{
    private static readonly string[] CompressedExtensions =
    [
        ".zip", ".gz", ".7z", ".rar", ".tar", ".tar.gz", ".tgz"
    ];

    public async Task<DecompressionResult> ScanAndDecompressAsync(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        if (!fileSystem.DirectoryExists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        var decompressedFiles = new List<string>();
        var compressedFilesList = new List<string>();
        var compressedFiles = fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
            .Where(file => CompressedExtensions.Any(ext =>
                file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

        foreach (var compressedFile in compressedFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var extractedFiles = await DecompressFileAsync(compressedFile, cancellationToken);
                decompressedFiles.AddRange(extractedFiles);
                compressedFilesList.Add(compressedFile);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.Error(ex, "Failed to decompress {CompressedFile}", compressedFile);
            }
        }

        return new DecompressionResult(decompressedFiles, compressedFilesList);
    }

    private async Task<List<string>> DecompressFileAsync(string filePath, CancellationToken cancellationToken)
    {
        var extractedFiles = new List<string>();
        var outputDirectory = Path.Combine(
            fileSystem.GetParentDirectory(filePath)!,
            fileSystem.GetFileNameWithoutExtension(filePath));

        fileSystem.CreateDirectory(outputDirectory);

        var extension = fileSystem.GetExtension(filePath).ToLowerInvariant();

        await Task.Run(() =>
        {
            if (extension == ".zip")
            {
                extractedFiles.AddRange(DecompressZip(filePath, outputDirectory, cancellationToken));
            }
            else if (extension == ".gz" || extension == ".tgz")
            {
                extractedFiles.AddRange(DecompressGZip(filePath, outputDirectory, cancellationToken));
            }
            else if (filePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
            {
                extractedFiles.AddRange(DecompressTarGz(filePath, outputDirectory, cancellationToken));
            }
            else if (extension == ".tar")
            {
                extractedFiles.AddRange(DecompressTar(filePath, outputDirectory, cancellationToken));
            }
        }, cancellationToken);

        return extractedFiles;
    }

    private List<string> DecompressZip(string zipPath, string outputDirectory, CancellationToken cancellationToken)
    {
        var extractedFiles = new List<string>();

        using var zipFile = new ZipFile(zipPath);
        foreach (ZipEntry entry in zipFile)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!entry.IsFile) continue;

            var entryFileName = Path.Combine(outputDirectory, entry.Name);
            var directoryName = fileSystem.GetFolderName(entryFileName);

            if (!string.IsNullOrEmpty(directoryName))
            {
                fileSystem.CreateDirectory(directoryName);
            }

            using var zipStream = zipFile.GetInputStream(entry);
            using var outputStream = fileSystem.CreateFile(entryFileName);
            zipStream.CopyTo(outputStream);
            extractedFiles.Add(entryFileName);
        }

        return extractedFiles;
    }

    private List<string> DecompressGZip(string gzipPath, string outputDirectory, CancellationToken cancellationToken)
    {
        var outputFileName = Path.Combine(outputDirectory,
            fileSystem.GetFileNameWithoutExtension(gzipPath));

        using var inputStream = fileSystem.OpenRead(gzipPath);
        using var gzipStream = new GZipInputStream(inputStream);
        using var outputStream = fileSystem.CreateFile(outputFileName);

        cancellationToken.ThrowIfCancellationRequested();
        gzipStream.CopyTo(outputStream);

        return [outputFileName];
    }

    private List<string> DecompressTar(string tarPath, string outputDirectory, CancellationToken cancellationToken)
    {
        var extractedFiles = new List<string>();

        using var inputStream = fileSystem.OpenRead(tarPath);
        using var tarArchive = TarArchive.CreateInputTarArchive(inputStream, null);

        cancellationToken.ThrowIfCancellationRequested();
        tarArchive.ExtractContents(outputDirectory);

        // Get all extracted files
        extractedFiles.AddRange(fileSystem.GetFiles(outputDirectory, "*.*", SearchOption.AllDirectories));

        return extractedFiles;
    }

    private List<string> DecompressTarGz(string tarGzPath, string outputDirectory, CancellationToken cancellationToken)
    {
        var extractedFiles = new List<string>();

        using var inputStream = fileSystem.OpenRead(tarGzPath);
        using var gzipStream = new GZipInputStream(inputStream);
        using var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, null);

        cancellationToken.ThrowIfCancellationRequested();
        tarArchive.ExtractContents(outputDirectory);

        extractedFiles.AddRange(fileSystem.GetFiles(outputDirectory, "*.*", SearchOption.AllDirectories));

        return extractedFiles;
    }
}
