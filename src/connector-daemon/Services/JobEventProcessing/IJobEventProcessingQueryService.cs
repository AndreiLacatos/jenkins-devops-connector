using connector_daemon.Services.JobEventProcessing.Models;

namespace connector_daemon.Services.JobEventProcessing;

internal interface IJobEventProcessingQueryService
{
    Task<IDictionary<string, IEnumerable<Job>>> ListRepositoryJobsByBranchAsync(string repository, CancellationToken cancellationToken);
}