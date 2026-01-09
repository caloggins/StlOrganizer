namespace StlOrganizer.Library;

public interface IFileSystem
{
    bool DirectoryExists(string path);
    IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption);
    void CreateDirectory(string path);
    string GetDirectoryName(string path);
    string GetFileNameWithoutExtension(string path);
    string GetExtension(string path);
    Stream OpenRead(string path);
    Stream CreateFile(string path);
}
