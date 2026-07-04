namespace connector_daemon.Services.JobEventProcessing;

internal interface IJobEventStatusNormalizer
{
    Task ResetStaleJobEventsAsync(CancellationToken cancellationToken);
}