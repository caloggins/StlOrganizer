namespace StlOrganizer.Library.SystemAdapters;

public class CancellationTokenSourceProvider : ICancellationTokenSourceProvider
{
    public CancellationTokenSource Create() => new CancellationTokenSource();
}
