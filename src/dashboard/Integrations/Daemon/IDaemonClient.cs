using connector_sytem.Common.ApiModels;
using connector_sytem.Common.ApiModels.Jobs;

namespace dashboard.Integrations.Daemon;

internal interface IDaemonClient
{
    Task<MonitoredRepositories> ListRepositoriesAsync(CancellationToken cancellationToken);
    Task<RepositoryJobs> ListRepositoryJobsAsync(string repository, CancellationToken cancellationToken);
    Task<Health> GetDaemonHealthAsync(CancellationToken cancellationToken);
    Task RequeueJobAsync(string name, int build, string commit, CancellationToken cancellationToken);
}