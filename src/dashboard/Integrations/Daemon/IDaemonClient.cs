using connector_sytem.Common.ApiModels.Jobs;
using dashboard.Integrations.Daemon.Models;

namespace dashboard.Integrations.Daemon;

internal interface IDaemonClient
{
    Task<MonitoredRepositories> ListRepositoriesAsync(CancellationToken cancellationToken);
    Task<RepositoryJobs> ListRepositoryJobsAsync(string repository, CancellationToken cancellationToken);
    Task<DaemonHealth> GetDaemonHealthAsync(CancellationToken cancellationToken);
}