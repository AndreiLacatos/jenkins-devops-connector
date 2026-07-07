using dashboard.Integrations.Azure.Models;

namespace dashboard.Integrations.Azure;

internal interface IAzureClient
{
    Task<IDictionary<AzureRepo, IReadOnlyList<AzurePullRequest>>> ListActivePullRequestsByRepositoriesAsync(
        IEnumerable<string> repoNames,
        CancellationToken cancellationToken);
}
