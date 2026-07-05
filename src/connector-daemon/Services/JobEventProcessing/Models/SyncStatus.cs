namespace connector_daemon.Services.JobEventProcessing.Models;

internal enum SyncStatus
{
    Pending,
    Enqueued,
    Processing,
    Completed,
    Failed,
}