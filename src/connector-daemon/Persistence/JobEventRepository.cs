using connector_daemon.Persistence.Entities;
using connector_daemon.Services.EventRegistration.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace connector_daemon.Persistence;

internal sealed class JobEventRepository : IJobEventRepository, IJobEventProcessingStatusRepository
{
    private readonly AppDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public JobEventRepository(
        AppDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<IEnumerable<JobEvent>> ListJobEventsAsync(
        JobEventListFilter filter,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.JobEvents.AsQueryable();
        if (filter.Build is not null)
        {
            query = query.Where(e => e.Build == filter.Build);
        }
        if (filter.JobName is not null)
        {
            query = query.Where(e => e.Name == filter.JobName);
        }
        if (filter.Commit is not null)
        {
            query = query.Where(e => e.Commit == filter.Commit);
        }
        return (await query.ToListAsync(cancellationToken))
            .Select(MapJobEvent);
    }

    public async Task SaveJobEventAsync(
        JobEvent jobEvent,
        CancellationToken cancellationToken)
    {
        await _dbContext.JobEvents.AddAsync(JobEventEntity.FromJobEvent(jobEvent), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<string>> ListMonitoredRepositoriesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.JobEvents.Select(e => e.GitUrl).Distinct().ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<JobEvent>> ListJobEventsAwaitingProcessingAsync(CancellationToken cancellationToken)
    {
        return (await _dbContext.JobEvents
            .Where(e => e.SyncStatus == SyncStatuses.Pending)
            .ToListAsync(cancellationToken))
            .Select(MapJobEvent);
    }

    public async Task<IEnumerable<JobEvent>> ListJobEventsInProcessingAsync(CancellationToken cancellationToken)
    {
        return (await _dbContext.JobEvents
                .Where(e => e.SyncStatus == SyncStatuses.Processing)
                .ToListAsync(cancellationToken))
            .Select(MapJobEvent);
    }

    public async Task<IEnumerable<JobEvent>> ListProcessedJobEvents(JobEventListFilter filter, CancellationToken cancellationToken)
    {
        var query = _dbContext.JobEvents
            .Where(e => e.SyncStatus == SyncStatuses.Completed || e.SyncStatus == SyncStatuses.Failed);
        if (filter.Build is not null)
        {
            query = query.Where(e => e.Build == filter.Build);
        }
        if (filter.JobName is not null)
        {
            query = query.Where(e => e.Name == filter.JobName);
        }
        if (filter.Commit is not null)
        {
            query = query.Where(e => e.Commit == filter.Commit);
        }
        return (await query.ToListAsync(cancellationToken))
            .Select(MapJobEvent);
    }

    public Task MarkJobEventPendingAsync(JobEvent jobEvent, CancellationToken cancellationToken)
        => UpdateJobEventAsync(
            jobEvent,
            SyncStatuses.Enqueued,
            s => s
                .SetProperty(e => e.EnqueuedAt, _timeProvider.GetUtcNow().ToString("O"))
                .SetProperty(e => e.FinishedAt, (string?)null),
            cancellationToken);

    public Task MarkJobEventEnqueuedAsync(
        JobEvent jobEvent,
        CancellationToken cancellationToken)
        => UpdateJobEventAsync(
            jobEvent,
            SyncStatuses.Enqueued,
            s => s.SetProperty(e => e.EnqueuedAt, _timeProvider.GetUtcNow().ToString("O")),
            cancellationToken);

    public Task MarkJobEventProcessingAsync(
        JobEvent jobEvent,
        CancellationToken cancellationToken)
        => UpdateJobEventAsync(
            jobEvent,
            SyncStatuses.Processing,
            s => s,
            cancellationToken);

    public Task MarkJobEventCompletedAsync(
        JobEvent jobEvent,
        CancellationToken cancellationToken)
        => UpdateJobEventAsync(
            jobEvent,
            SyncStatuses.Completed,
            s => s.SetProperty(e => e.FinishedAt, _timeProvider.GetUtcNow().ToString("O")),
            cancellationToken);

    public Task MarkJobEventFailedAsync(
        JobEvent jobEvent,
        CancellationToken cancellationToken)
        => UpdateJobEventAsync(
            jobEvent,
            SyncStatuses.Failed,
            s => s.SetProperty(e => e.FinishedAt, _timeProvider.GetUtcNow().ToString("O")),
            cancellationToken);

    private Task<int> UpdateJobEventAsync(
        JobEvent jobEvent,
        string syncStatus,
        Func<UpdateSettersBuilder<JobEventEntity>, UpdateSettersBuilder<JobEventEntity>> additionalUpdates,
        CancellationToken cancellationToken)
    {
        return FindJobEvent(jobEvent)
            .ExecuteUpdateAsync(
                s => additionalUpdates(
                    s.SetProperty(e => e.SyncStatus, syncStatus)),
                cancellationToken);
    }

    private IQueryable<JobEventEntity> FindJobEvent(JobEvent jobEvent) =>
        _dbContext.JobEvents.Where(e =>
            e.Name == jobEvent.Name &&
            e.Build == jobEvent.Build &&
            e.Commit == jobEvent.Commit &&
            e.JobEvent == MapStatus(jobEvent.Status));

    private static JobEvent MapJobEvent(JobEventEntity e) => new JobEvent
    {
        Name = e.Name,
        Build = e.Build,
        Commit = e.Commit,
        GitUrl = e.GitUrl,
        Status = MapStatus(e.JobEvent),
        RegisteredAt = DateTimeOffset.Parse(e.RegisteredAt),
        BuildUrl = e.Url,
    };

    private static string MapStatus(JobStatus s) => s switch
    {
        JobStatus.Started => JobEvents.Started,
        JobStatus.Succeeded => JobEvents.Succeeded,
        JobStatus.Failed => JobEvents.Failed,
        _ => JobEvents.Aborted,
    };

    private static JobStatus MapStatus(string s) => s switch
    {
        JobEvents.Started => JobStatus.Started,
        JobEvents.Succeeded => JobStatus.Succeeded,
        JobEvents.Failed => JobStatus.Failed,
        _ => JobStatus.Aborted,
    };
}
