using connector_daemon.Persistence;
using connector_daemon.Services.JobEventProcessing.Models;

namespace connector_daemon.Services.JobEventProcessing;

internal sealed class JobEventProcessingQueryService : IJobEventProcessingQueryService
{
    private readonly IJobEventProcessingStatusRepository _repository;

    public JobEventProcessingQueryService(
        IJobEventProcessingStatusRepository repository)
    {
        _repository = repository;
    }

    public async Task<IDictionary<string, IEnumerable<Job>>> ListRepositoryJobsByBranchAsync(
        string repository, CancellationToken cancellationToken)
    {
        var jobs = await _repository.ListJobsAsync(
            new JobListFilter
            {
                Repository =  repository,
            },
            cancellationToken);
        return jobs
            .GroupBy(j => j.Branch)
            .ToDictionary(g => g.Key, IEnumerable<Job> (g) => g.ToList());
    }
}
