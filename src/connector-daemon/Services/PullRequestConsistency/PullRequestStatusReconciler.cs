using connector_daemon.AzureIntegration;
using connector_daemon.Persistence;
using connector_daemon.Services.JobEventProcessing;

namespace connector_daemon.Services.PullRequestConsistency;

internal sealed class PullRequestStatusReconciler : IPullRequestStatusReconciler
{
    private readonly ILogger<PullRequestStatusReconciler> _logger;
    private readonly IAzureClient _azureClient;
    private readonly IJobEventRepository _jobEventRepository;
    private readonly MonitoredRepositories _monitoredRepositories;

    public PullRequestStatusReconciler(
        ILogger<PullRequestStatusReconciler> logger,
        IAzureClient azureClient,
        IJobEventRepository jobEventRepository,
        MonitoredRepositories monitoredRepositories)
    {
        _logger = logger;
        _azureClient = azureClient;
        _jobEventRepository = jobEventRepository;
        _monitoredRepositories = monitoredRepositories;
    }

    public async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        foreach (var azureRepo in _monitoredRepositories)
        {
            _logger.LogInformation(
                "Reconciling repository '{RepoName}' (project '{ProjectName}') pull requests",
                azureRepo.Name,
                azureRepo.Project.Name);
            var pullRequests = await _azureClient.ListActivePullRequestsAsync(azureRepo, cancellationToken);
            foreach (var pullRequest in pullRequests)
            {
                if (pullRequest.LatestCommit?.LatestJenkinsStatus is null)
                {
                    // there are either no commits in this PR or no Jenkins status had been set yet 
                    continue;
                }

                if (pullRequest.LatestJenkinsStatus is not null)
                {
                    // Jenkins status has already been synced 
                    continue;
                }

                var jobEvents = await _jobEventRepository.ListJobEventsAsync(
                    new JobEventListFilter
                    {
                        Commit = pullRequest.LatestCommit.Id,
                    },
                    cancellationToken);
                var mostRecentEvent = jobEvents.MaxBy(j => j.RegisteredAt);
                if (mostRecentEvent is not null)
                {
                    await _azureClient.SetPrStatusAsync(azureRepo, pullRequest, mostRecentEvent, cancellationToken);
                    var commenter = new PullRequestCommenter(_azureClient);
                    await commenter.AddPipelineStatusCommentAsync(azureRepo, pullRequest, mostRecentEvent, cancellationToken);
                }
            }
        }
    }
}
