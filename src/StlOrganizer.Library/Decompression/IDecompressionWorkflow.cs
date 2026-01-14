namespace StlOrganizer.Library.Decompression;

public interface IDecompressionWorkflow
{
    Task Execute(
        string directoryPath,
        CancellationToken cancellationToken = default);
}
