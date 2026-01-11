namespace StlOrganizer.Library.SystemFileAdapters;

public interface IFileSystem
{
    bool DirectoryExists(string path);
    IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption);
    string[] GetDirectories(string path);
    void CreateDirectory(string path);
    string GetDirectoryName(string path);
    string? GetParentDirectory(string path);
    string CombinePaths(params string[] paths);
    string GetFileNameWithoutExtension(string path);
    string GetExtension(string path);
    Stream OpenRead(string path);
    Stream CreateFile(string path);
}
