namespace connector_daemon.Services.EventRegistration.Models;

internal sealed class JobEventRequest
{
    public required string Name { get; init; }
    public required  int Build { get; init; }
    public required string Commit { get; init; }
    public required JobStatus Status { get; init; }
    public required string? Url { get; init; }
}
