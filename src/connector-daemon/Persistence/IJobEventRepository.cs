using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Persistence;

internal interface IJobEventRepository
{
    Task<IEnumerable<JobEvent>> ListJobEventsAsync(JobEventListFilter filter, CancellationToken cancellationToken);

    Task SaveJobEventAsync(JobEvent jobEvent, CancellationToken cancellationToken);
}
