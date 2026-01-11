namespace StlOrganizer.Library.Decompression;

public interface IFileDecompressor
{
    Task<IEnumerable<string>> ScanAndDecompressAsync(string directoryPath);
}
