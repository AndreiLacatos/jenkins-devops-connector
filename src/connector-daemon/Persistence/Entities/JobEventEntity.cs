using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Persistence.Entities;

internal sealed class JobEventEntity
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Build { get; set; }
    public string GitUrl { get; set; }
    public string Commit { get; set; }
    public string JobEvent { get; set; }
    public string? Url { get; set; }
    public string RegisteredAt { get; set; }
    public string SyncStatus { get; set; }
    public string? EnqueuedAt { get; set; }
    public string? FinishedAt { get; set; }

    internal static JobEventEntity FromJobEvent(JobEvent jobEvent) => new JobEventEntity
    {
        Name = jobEvent.Name,
        Build = jobEvent.Build,
        Commit = jobEvent.Commit,
        GitUrl = jobEvent.GitUrl,
        JobEvent = jobEvent.Status switch
        {
            JobStatus.Started => JobEvents.Started,
            JobStatus.Succeeded => JobEvents.Succeeded,
            JobStatus.Failed => JobEvents.Failed,
            _ => JobEvents.Aborted,
        },
        Url = jobEvent.BuildUrl,
        RegisteredAt = jobEvent.RegisteredAt.ToString("O"),
        SyncStatus = SyncStatuses.Pending,
        EnqueuedAt = null,
        FinishedAt = null,
    };
}
