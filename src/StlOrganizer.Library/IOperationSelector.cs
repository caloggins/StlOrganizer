namespace StlOrganizer.Library;

public interface IOperationSelector
{
    Task<string> ExecuteOperationAsync(FileOperation operationType, string directoryPath, CancellationToken ct);
}
