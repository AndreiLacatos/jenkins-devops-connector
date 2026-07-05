using connector_daemon.Persistence;
using connector_daemon.Services.JobEventProcessing.Models;

namespace connector_daemon.Services.JobEventProcessing;

internal sealed class JobEventProcessingQueryService : IJobEventProcessingQueryService
{
    public JobEventProcessingQueryService(
        IJobEventProcessingStatusRepository repository)
    {
        
    }

    public Task<IDictionary<string, IEnumerable<Job>>> ListRepositoryJobsByBracnAsync(
        string repository, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}