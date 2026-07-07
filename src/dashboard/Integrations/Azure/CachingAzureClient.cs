using System.Collections.Concurrent;
using dashboard.Integrations.Azure.Models;

namespace dashboard.Integrations.Azure;

internal sealed class CachingAzureClient : IAzureClient
{
    private static readonly TimeSpan RepoCacheTimeout = TimeSpan.FromHours(6);
    private static readonly TimeSpan PullRequestCacheTimeout = TimeSpan.FromMinutes(2);

    private readonly AzureClient _azureClient;
    private readonly TimeProvider _timeProvider;

    private readonly CacheEntry<IReadOnlyList<AzureRepo>> _repoCache = new();

    private readonly ConcurrentDictionary<string, CacheEntry<IReadOnlyList<AzurePullRequest>>> _pullRequestCache =
        new();

    public CachingAzureClient(
        AzureClient azureClient,
        TimeProvider timeProvider)
    {
        _azureClient = azureClient;
        _timeProvider = timeProvider;
    }

    public async Task<IDictionary<AzureRepo, IReadOnlyList<AzurePullRequest>>> ListActivePullRequestsByRepositoriesAsync(
        IEnumerable<string> repoNames,
        CancellationToken cancellationToken)
    {
        var repos = await GetRepositories(cancellationToken);

        var repoNameSet = repoNames.ToHashSet(StringComparer.Ordinal);

        var targetRepos = repos
            .Where(repo => repoNameSet.Contains(repo.Name))
            .ToArray();

        var pullRequests = await Task.WhenAll(
            targetRepos.Select(repo => GetPullRequests(repo, cancellationToken)));

        return targetRepos
            .Zip(pullRequests)
            .ToDictionary(
                x => x.First,
                x => x.Second);
    }

    private async Task<IReadOnlyList<AzureRepo>> GetRepositories(
        CancellationToken cancellationToken)
    {
        if (!_repoCache.IsExpired(_timeProvider))
        {
            return _repoCache.Value!;
        }

        await _repoCache.Lock.WaitAsync(cancellationToken);
        try
        {
            if (!_repoCache.IsExpired(_timeProvider))
            {
                return _repoCache.Value!;
            }

            var repos = (await _azureClient.ListVisibleRepositories(cancellationToken))
                .ToArray();

            _repoCache.Value = repos;
            _repoCache.ExpiresAt = _timeProvider.GetUtcNow().Add(RepoCacheTimeout);

            return repos;
        }
        finally
        {
            _repoCache.Lock.Release();
        }
    }

    private async Task<IReadOnlyList<AzurePullRequest>> GetPullRequests(
        AzureRepo repo,
        CancellationToken cancellationToken)
    {
        var cacheEntry = _pullRequestCache.GetOrAdd(
            repo.Id,
            _ => new CacheEntry<IReadOnlyList<AzurePullRequest>>());

        if (!cacheEntry.IsExpired(_timeProvider))
        {
            return cacheEntry.Value!;
        }

        await cacheEntry.Lock.WaitAsync(cancellationToken);
        try
        {
            if (!cacheEntry.IsExpired(_timeProvider))
            {
                return cacheEntry.Value!;
            }

            var pullRequests = (await _azureClient.ListActivePullRequestsAsync(
                    repo,
                    cancellationToken))
                .ToArray();

            cacheEntry.Value = pullRequests;
            cacheEntry.ExpiresAt = _timeProvider.GetUtcNow().Add(PullRequestCacheTimeout);

            return pullRequests;
        }
        finally
        {
            cacheEntry.Lock.Release();
        }
    }

    private sealed class CacheEntry<T>
    {
        public SemaphoreSlim Lock { get; } = new(1, 1);

        public T? Value { get; set; }

        public DateTimeOffset ExpiresAt { get; set; }

        public bool IsExpired(TimeProvider timeProvider) =>
            Value is null || ExpiresAt <= timeProvider.GetUtcNow();
    }
}