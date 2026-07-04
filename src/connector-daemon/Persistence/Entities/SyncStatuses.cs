namespace connector_daemon.Persistence.Entities;

internal static class SyncStatuses
{
    internal const string Pending = "pending";
    internal const string Enqueued = "enqueued";
    internal const string Processing = "processing";
    internal const string Completed = "completed";
    internal const string Failed = "failed";
}