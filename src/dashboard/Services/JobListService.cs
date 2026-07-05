using connector_sytem.Common.ApiModels.Jobs;
using dashboard.Integrations.Daemon;
using dashboard.Services.Models;
using RepositoryJobs = dashboard.Services.Models.RepositoryJobs;

namespace dashboard.Services;

internal class JobListService : IJobListService
{
    private readonly IDaemonClient _daemonClient;

    public JobListService(IDaemonClient daemonClient)
    {
        _daemonClient = daemonClient;
    }

    public async Task<IEnumerable<RepositoryJobs>> ListMostRecentJobsByRepositoriesAsync(CancellationToken cancellationToken)
    {
        var result = new List<RepositoryJobs>();
        var repos = await _daemonClient.ListRepositoriesAsync(cancellationToken);
        foreach (var repo in repos.Repositories)
        {
            var repoJobs = await _daemonClient.ListRepositoryJobsAsync(repo.Name, cancellationToken);
            result.Add(new RepositoryJobs
            {
                RepositoryName = repoJobs.RepositoryName,
                BranchJobs = repoJobs.BranchJobs.ToDictionary(
                    kvp => kvp.Key,
                    kvp => Map(kvp.Value.MaxBy(j => DateTimeOffset.Parse(j.RegisteredAt))!)),
            });
        }

        return result;
    }

    private static Job Map(SyncJob job) => new Job
    {
        Build = job.Build,
        Commit = job.Commit,
        BuildUrl = job.BuildUrl,
        EnqueuedAt = job.EnqueuedAt,
        FinishedAt = job.FinishedAt,
        Name = job.Name,
        RegisteredAt = job.RegisteredAt,
        SyncStatus = job.SyncStatus,
        JobEvent = job.JenkinsStatus,
    };
}
