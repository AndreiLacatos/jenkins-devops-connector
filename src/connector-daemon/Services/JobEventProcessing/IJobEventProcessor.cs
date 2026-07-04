using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Services.JobEventProcessing;

internal interface IJobEventProcessor
{
    Task ProcessJobEventAsync(JobEvent jobEvent, CancellationToken cancellationToken);
}
