namespace connector_daemon.Services.EventRegistration.Models;

internal sealed class JobEventRequeueRequest
{
    public required string Name { get; init; }
    public required  int Build { get; init; }
    public required string Commit { get; init; }
}
