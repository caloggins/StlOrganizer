namespace StlOrganizer.Library.SystemFileAdapters;

public class FileSystemAdapter : IFileSystem
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public IEnumerable<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        => Directory.GetFiles(path, searchPattern, searchOption);

    public string[] GetDirectories(string path) => Directory.GetDirectories(path);

    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    public string GetDirectoryName(string path) => Path.GetDirectoryName(path) ?? string.Empty;

    public string? GetParentDirectory(string path) => Path.GetDirectoryName(path);

    public string CombinePaths(params string[] paths) => Path.Combine(paths);

    public string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

    public string GetExtension(string path) => Path.GetExtension(path);

    public Stream OpenRead(string path) => File.OpenRead(path);

    public Stream CreateFile(string path) => File.Create(path);
}
