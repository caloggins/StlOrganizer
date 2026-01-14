using StlOrganizer.Library.OperationSelection;

namespace StlOrganizer.Library.Decompression;

public interface IDecompressionWorkflow
{
    Task Execute(
        string directoryPath,
        IProgress<OrganizerProgress> progress,
        CancellationToken cancellationToken = default);
}
