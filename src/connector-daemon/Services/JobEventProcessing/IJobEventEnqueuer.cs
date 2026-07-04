namespace connector_daemon.Services.JobEventProcessing;

internal interface IJobEventEnqueuer
{
    Task EnqueuePendingJobs(CancellationToken cancellationToken);
}
