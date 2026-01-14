using StlOrganizer.Library.SystemAdapters;

namespace StlOrganizer.Library.OperationSelection;

public sealed class ArchiveOperation : SmartEnum<ArchiveOperation>
{
    public static readonly ArchiveOperation DecompressArchives = new(1, "Decompress files");
    public static readonly ArchiveOperation CompressFolder = new(2, "Compress folder");
    public static readonly ArchiveOperation ExtractImages = new(3, "Extract images");

    private ArchiveOperation(int id, string name) : base(id, name)
    {
    }

    public static ArchiveOperation FromId(int operation)
    {
        return operation switch
        {
            1 => DecompressArchives,
            2 => CompressFolder,
            3 => ExtractImages,
            _ => throw new ArgumentException($"Invalid operation ID: {operation}", nameof(operation))
        };
    }
}