using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using Serilog;
using ZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace StlOrganizer.Library;

public class FileDecompressor
{
    private readonly IFileSystem fileSystem;
    private readonly ILogger logger;
    private static readonly string[] CompressedExtensions =
    {
        ".zip", ".gz", ".7z", ".rar", ".tar", ".tar.gz", ".tgz"
    };

    public FileDecompressor() : this(new FileSystemAdapter(), Log.Logger)
    {
    }

    public FileDecompressor(IFileSystem fileSystem, ILogger logger)
    {
        this.fileSystem = fileSystem;
        this.logger = logger;
    }

    public async Task<IEnumerable<string>> ScanAndDecompressAsync(string directoryPath)
    {
        if (!fileSystem.DirectoryExists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");
        }

        var decompressedFiles = new List<string>();
        var compressedFiles = fileSystem.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
            .Where(file => CompressedExtensions.Any(ext =>
                file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

        foreach (var compressedFile in compressedFiles)
        {
            try
            {
                var extractedFiles = await DecompressFileAsync(compressedFile);
                decompressedFiles.AddRange(extractedFiles);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to decompress {CompressedFile}", compressedFile);
            }
        }

        return decompressedFiles;
    }

    private async Task<List<string>> DecompressFileAsync(string filePath)
    {
        var extractedFiles = new List<string>();
        var outputDirectory = Path.Combine(
            fileSystem.GetDirectoryName(filePath),
            fileSystem.GetFileNameWithoutExtension(filePath));

        fileSystem.CreateDirectory(outputDirectory);

        var extension = fileSystem.GetExtension(filePath).ToLowerInvariant();

        await Task.Run(() =>
        {
            if (extension == ".zip")
            {
                extractedFiles.AddRange(DecompressZip(filePath, outputDirectory));
            }
            else if (extension == ".gz" || extension == ".tgz")
            {
                extractedFiles.AddRange(DecompressGZip(filePath, outputDirectory));
            }
            else if (filePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
            {
                extractedFiles.AddRange(DecompressTarGz(filePath, outputDirectory));
            }
            else if (extension == ".tar")
            {
                extractedFiles.AddRange(DecompressTar(filePath, outputDirectory));
            }
        });

        return extractedFiles;
    }

    private List<string> DecompressZip(string zipPath, string outputDirectory)
    {
        var extractedFiles = new List<string>();

        using var zipFile = new ZipFile(zipPath);
        foreach (ZipEntry entry in zipFile)
        {
            if (!entry.IsFile) continue;

            var entryFileName = Path.Combine(outputDirectory, entry.Name);
            var directoryName = fileSystem.GetDirectoryName(entryFileName);

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

    private List<string> DecompressGZip(string gzipPath, string outputDirectory)
    {
        var outputFileName = Path.Combine(outputDirectory,
            fileSystem.GetFileNameWithoutExtension(gzipPath));

        using var inputStream = fileSystem.OpenRead(gzipPath);
        using var gzipStream = new GZipInputStream(inputStream);
        using var outputStream = fileSystem.CreateFile(outputFileName);
        gzipStream.CopyTo(outputStream);

        return new List<string> { outputFileName };
    }

    private List<string> DecompressTar(string tarPath, string outputDirectory)
    {
        var extractedFiles = new List<string>();

        using var inputStream = fileSystem.OpenRead(tarPath);
        using var tarArchive = TarArchive.CreateInputTarArchive(inputStream, null);
        tarArchive.ExtractContents(outputDirectory);

        // Get all extracted files
        extractedFiles.AddRange(fileSystem.GetFiles(outputDirectory, "*.*", SearchOption.AllDirectories));

        return extractedFiles;
    }

    private List<string> DecompressTarGz(string tarGzPath, string outputDirectory)
    {
        var extractedFiles = new List<string>();

        using var inputStream = fileSystem.OpenRead(tarGzPath);
        using var gzipStream = new GZipInputStream(inputStream);
        using var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, null);
        tarArchive.ExtractContents(outputDirectory);

        extractedFiles.AddRange(fileSystem.GetFiles(outputDirectory, "*.*", SearchOption.AllDirectories));

        return extractedFiles;
    }
}
