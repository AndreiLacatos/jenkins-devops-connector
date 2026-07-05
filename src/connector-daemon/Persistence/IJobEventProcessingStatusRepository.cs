using connector_daemon.Services.EventRegistration.Models;
using connector_daemon.Services.JobEventProcessing.Models;

namespace connector_daemon.Persistence;

internal interface IJobEventProcessingStatusRepository
{
    Task<IEnumerable<JobEvent>> ListJobEventsAwaitingProcessingAsync(CancellationToken cancellationToken);

    Task<IEnumerable<JobEvent>> ListJobEventsInProcessingAsync(CancellationToken cancellationToken);

    Task<IEnumerable<JobEvent>> ListProcessedJobEvents(JobEventListFilter filter, CancellationToken cancellationToken);

    Task<IEnumerable<Job>> ListJobsAsync(JobListFilter filter, CancellationToken cancellationToken);

    Task MarkJobEventPendingAsync(JobEvent jobEvent, CancellationToken cancellationToken);

    Task MarkJobEventEnqueuedAsync(JobEvent jobEvent, CancellationToken cancellationToken);

    Task MarkJobEventProcessingAsync(JobEvent jobEvent, CancellationToken cancellationToken);

    Task MarkJobEventCompletedAsync(JobEvent jobEvent, CancellationToken cancellationToken);

    Task MarkJobEventFailedAsync(JobEvent jobEvent, CancellationToken cancellationToken);
}