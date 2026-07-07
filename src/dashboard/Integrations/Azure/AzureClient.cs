using dashboard.Integrations.Azure.Models;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace dashboard.Integrations.Azure;

internal sealed class AzureClient : IAzureClient, IDisposable
{
    public AzureClient(IOptions<AzureClientOptions> options)
    {
        var credentials = new VssBasicCredential(string.Empty, options.Value.Pat);
        _connection = new VssConnection(
            new Uri(options.Value.OrganizationUrl),
            credentials);
        _gitClient = _connection.GetClient<GitHttpClient>();
    }

    private readonly VssConnection _connection;
    private readonly GitHttpClient _gitClient;

    public async Task<IDictionary<AzureRepo, IReadOnlyList<AzurePullRequest>>> ListActivePullRequestsByRepositoriesAsync(
        IEnumerable<string> repoNames,
        CancellationToken cancellationToken)
    {
        var repos = await ListVisibleRepositories(cancellationToken);
        var repoNameSet = repoNames.ToHashSet(StringComparer.Ordinal);
        var targetRepos = repos.Where(repo => repoNameSet.Contains(repo.Name)).ToArray();
        var prs = await Task.WhenAll(targetRepos.Select(repo => ListActivePullRequestsAsync(repo, cancellationToken)));
        return targetRepos
            .Zip(prs)
            .ToDictionary(
                x => x.First,
                IReadOnlyList<AzurePullRequest> (x) => x.Second.ToArray().AsReadOnly());
    }

    public void Dispose()
    {
        _connection.Dispose();
        _gitClient.Dispose();
    }

    internal async Task<IEnumerable<AzureRepo>> ListVisibleRepositories(CancellationToken cancellationToken)
    {
        var repos = await _gitClient.GetRepositoriesAsync(cancellationToken: cancellationToken);
        return repos.Select(repo => new AzureRepo
        {
            Id = repo.Id.ToString(),
            Name = repo.Name,
        });
    }

    internal async Task<IEnumerable<AzurePullRequest>> ListActivePullRequestsAsync(AzureRepo repo, CancellationToken cancellationToken)
    {
        var pullRequests = await _gitClient.GetPullRequestsAsync(
            repo.Id,
            new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Active,
            },
            cancellationToken: cancellationToken);
        
        return pullRequests.Select(pr => new AzurePullRequest
        {
            Id = pr.PullRequestId,
            Title = pr.Title,
            SourceBranch = pr.SourceRefName,
        });
    }
}
