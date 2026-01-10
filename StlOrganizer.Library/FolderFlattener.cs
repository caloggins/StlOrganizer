namespace StlOrganizer.Library;

public class FolderFlattener(IDirectoryService directoryService)
{
    public FolderFlattener() : this(new DirectoryServiceAdapter())
    {
    }

    public void RemoveNestedFolders(string rootPath)
    {
        if (!directoryService.Exists(rootPath))
            throw new DirectoryNotFoundException($"Directory not found: {rootPath}");

        ProcessDirectory(rootPath);
    }

    private void ProcessDirectory(string directoryPath)
    {
        var subdirectories = directoryService.GetDirectories(directoryPath);

        foreach (var subdirectory in subdirectories)
        {
            ProcessDirectory(subdirectory);
            FlattenIfMatching(directoryPath, subdirectory);
        }
    }

    private void FlattenIfMatching(string parentPath, string childPath)
    {
        var parentName = directoryService.GetDirectoryName(parentPath);
        var childName = directoryService.GetDirectoryName(childPath);

        if (string.Equals(parentName, childName, StringComparison.OrdinalIgnoreCase))
        {
            directoryService.Move(childPath, parentPath);
            directoryService.Delete(childPath, recursive: false);
        }
    }
}
