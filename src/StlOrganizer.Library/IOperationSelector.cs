namespace StlOrganizer.Library;

public interface IOperationSelector
{
    Task<string> ExecuteOperationAsync(OperationType operationType, string directoryPath);
}
