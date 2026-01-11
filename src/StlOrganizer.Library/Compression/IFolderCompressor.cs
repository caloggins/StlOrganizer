namespace StlOrganizer.Library.Compression;

public interface IFolderCompressor
{
    string CompressFolder(string folderPath, string? outputPath = null);
}
