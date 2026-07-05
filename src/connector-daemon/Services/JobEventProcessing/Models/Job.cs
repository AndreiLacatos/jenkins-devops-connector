using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Services.JobEventProcessing.Models;

internal sealed class Job
{
    public required string Name { get; init; }
    public required int Build { get; init; }
    public required string GitUrl { get; init; }
    public required string Commit { get; init; }
    public required string Branch { get; init; }
    public required JobStatus Status { get; init; }
    public required string? BuildUrl { get; init; }
    public required DateTimeOffset RegisteredAt { get; set; }
    public required SyncStatus SyncStatus { get; set; }
    public required DateTimeOffset? EnqueuedAt { get; set; }
    public required DateTimeOffset? FinishedAt { get; set; }
}
