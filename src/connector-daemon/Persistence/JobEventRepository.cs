using connector_daemon.Persistence.Entities;
using connector_daemon.Services.EventRegistration.Models;
using Microsoft.EntityFrameworkCore;

namespace connector_daemon.Persistence;

internal class JobEventRepository : IJobEventRepository
{
    private readonly AppDbContext _dbContext;

    public JobEventRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
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
            .Select(e => new JobEvent
            {
                Name = e.Name,
                Build = e.Build,
                Commit = e.Commit,
                Status = e.JobEvent switch
                {
                    JobEvents.Started => JobStatus.Started,
                    JobEvents.Succeeded => JobStatus.Succeeded,
                    JobEvents.Failed => JobStatus.Failed,
                    _ => JobStatus.Aborted,
                },
                RegisteredAt = DateTimeOffset.Parse(e.RegisteredAt),
                Url = e.Url,
            });
    }

    public async Task SaveJobEventAsync(
        JobEvent jobEvent,
        CancellationToken cancellationToken)
    {
        await _dbContext.JobEvents.AddAsync(JobEventEntity.FromJobEvent(jobEvent), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
