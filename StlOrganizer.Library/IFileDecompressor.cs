namespace StlOrganizer.Library;

public interface IFileDecompressor
{
    Task<IEnumerable<string>> ScanAndDecompressAsync(string directoryPath);
}
