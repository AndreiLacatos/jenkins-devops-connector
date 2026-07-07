using connector_sytem.Common.ApiModels.Jobs;
using dashboard.Integrations.Azure;
using dashboard.Integrations.Daemon;
using dashboard.Services.Models;
using RepositoryJobs = dashboard.Services.Models.RepositoryJobs;

namespace dashboard.Services;

internal class JobListService : IJobListService
{
    private readonly IDaemonClient _daemonClient;
    private readonly IAzureClient _azureClient;

    public JobListService(
        IDaemonClient daemonClient,
        IAzureClient azureClient)
    {
        _daemonClient = daemonClient;
        _azureClient = azureClient;
    }

    public async Task<IEnumerable<RepositoryJobs>> ListMostRecentJobsByRepositoriesAsync(CancellationToken cancellationToken)
    {
        var result = new List<RepositoryJobs>();
        var monitoredRepos = await _daemonClient.ListRepositoriesAsync(cancellationToken);
        var azureRepos = await _azureClient.ListActivePullRequestsByRepositoriesAsync(
            monitoredRepos.Repositories.Select(repo => repo.Name),
            cancellationToken);

        foreach (var repo in monitoredRepos.Repositories)
        {
            var prs = azureRepos.FirstOrDefault(x => x.Key.Name == repo.Name).Value;
            if (prs is null)
            {
                // repo no longer in Azure, fishy, skipping
                continue;
            }

            prs = prs.ToArray();
            if (!prs.Any())
            {
                // there are no PRs in this repo, skip listing jobs as the ones are already irrelevant
                result.Add(new RepositoryJobs
                {
                    RepositoryName = repo.Name,
                    BranchJobs = [],
                });
            }
            else
            {
                // list jobs and merge them with the active Azure PRs (show items for active PRs only)
                var repoJobs = await _daemonClient.ListRepositoryJobsAsync(repo.Name, cancellationToken);
                var azureBranchesWithPrs = prs.Select(pr => NormalizeBranchName(pr.SourceBranch)).ToHashSet();
                var jobsByActiveBranches = repoJobs.BranchJobs
                    .Where(kvp => azureBranchesWithPrs.Contains(NormalizeBranchName(kvp.Key)))
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => Map(kvp.Value.MaxBy(j => DateTimeOffset.Parse(j.RegisteredAt))!));
                result.Add(new RepositoryJobs
                {
                    RepositoryName = repoJobs.RepositoryName,
                    BranchJobs = jobsByActiveBranches,
                });
            }
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

    private static string NormalizeBranchName(string branch)
    {
        const string refsHeadsPrefix = "refs/heads/";
        const string refsRemotesPrefix = "refs/remotes/";

        if (branch.StartsWith(refsHeadsPrefix, StringComparison.OrdinalIgnoreCase))
        {
            branch = branch[refsHeadsPrefix.Length..];
        }
        else if (branch.StartsWith(refsRemotesPrefix, StringComparison.OrdinalIgnoreCase))
        {
            branch = branch[refsRemotesPrefix.Length..];

            // origin/foo -> foo
            var slashIndex = branch.IndexOf('/');
            if (slashIndex >= 0)
            {
                branch = branch[(slashIndex + 1)..];
            }
        }

        return branch.Trim('/');
    }
}
