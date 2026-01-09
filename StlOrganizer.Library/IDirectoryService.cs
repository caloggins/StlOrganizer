namespace StlOrganizer.Library;

public interface IDirectoryService
{
    bool Exists(string path);
    string[] GetDirectories(string path);
    string[] GetFiles(string path);
    void Move(string sourcePath, string destinationPath);
    void Delete(string path, bool recursive);
    string GetDirectoryName(string path);
}
