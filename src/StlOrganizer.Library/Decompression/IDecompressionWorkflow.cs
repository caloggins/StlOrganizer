namespace StlOrganizer.Library.Decompression;

public interface IDecompressionWorkflow
{
    Task<IEnumerable<string>> ExecuteAsync(string directoryPath);
}
