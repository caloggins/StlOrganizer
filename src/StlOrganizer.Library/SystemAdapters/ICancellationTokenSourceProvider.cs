namespace StlOrganizer.Library.SystemAdapters;

public interface ICancellationTokenSourceProvider
{
    CancellationTokenSource Create();
}