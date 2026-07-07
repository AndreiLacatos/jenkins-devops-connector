using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Services.EventRegistration;

internal interface IJobEventRegistrar
{
    Task RegisterJobEventAsync(JobEventRequest request, CancellationToken cancellationToken);
    Task RequeueJobEventAsync(JobEventRequeueRequest request, CancellationToken cancellationToken);
}
