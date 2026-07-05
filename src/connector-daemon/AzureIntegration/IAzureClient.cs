using connector_daemon.AzureIntegration.Models;
using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.AzureIntegration;

internal interface IAzureClient
{
    Task<AzureRepo> GetRepositoryAsync(string repoUrl, CancellationToken cancellationToken);
    Task<AzureCommit> GetCommitAsync(AzureRepo repo, string commit, CancellationToken cancellationToken);
    Task<IEnumerable<AzurePullRequest>> ListActivePullRequestsAsync(
        AzureRepo repo, CancellationToken cancellationToken);
    Task<IEnumerable<AzurePullRequest>> ListAssociatedActivePullRequestsAsync(
        AzureRepo repo, AzureCommit commit, CancellationToken cancellationToken);
    Task SetCommitStatusAsync(AzureRepo repo, AzureCommit commit, JobEvent status, CancellationToken cancellationToken);
    Task SetPrStatusAsync(AzureRepo repo, AzurePullRequest pr, JobEvent status, CancellationToken cancellationToken);
}
