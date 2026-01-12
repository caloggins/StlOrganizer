namespace StlOrganizer.Library;

public abstract class SmartEnum(int id, string name)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
}

public sealed class FileOperation : SmartEnum
{
    public static readonly FileOperation DecompressFiles = new(1, "Decompress files");
    public static readonly FileOperation CompressFolder = new(2, "Compress folder");
    public static readonly FileOperation ExtractImages = new(3, "Extract images");

    private FileOperation(int id, string name) : base(id, name)
    {
    }
    
    public static implicit operator int(FileOperation operation) => operation.Id;
    public static implicit operator string(FileOperation operation) => operation.Name;
}