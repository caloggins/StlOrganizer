namespace StlOrganizer.Library.OperationSelection;

public sealed record OrganizerProgress
{
    public int Progress { get; init; } = 0;
    public string? Message { get; init; } = "";
}
