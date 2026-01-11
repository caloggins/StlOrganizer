namespace StlOrganizer.Library.SystemFileAdapters;

public class DirectoryServiceAdapter : IDirectoryService
{
    public bool Exists(string path) => Directory.Exists(path);

    public string[] GetDirectories(string path) => Directory.GetDirectories(path);

    public string[] GetFiles(string path) => Directory.GetFiles(path);

    public void Move(string sourcePath, string destinationPath)
    {
        foreach (var file in Directory.GetFiles(sourcePath))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destinationPath, fileName);
            File.Move(file, destFile);
        }

        foreach (var dir in Directory.GetDirectories(sourcePath))
        {
            var dirName = Path.GetFileName(dir);
            var destDir = Path.Combine(destinationPath, dirName);
            Directory.Move(dir, destDir);
        }
    }

    public void Delete(string path, bool recursive) => Directory.Delete(path, recursive);

    public string GetDirectoryName(string path) => Path.GetFileName(path);
}
