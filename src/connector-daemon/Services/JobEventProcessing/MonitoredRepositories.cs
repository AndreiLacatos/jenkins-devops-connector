using System.Collections;
using connector_daemon.AzureIntegration.Models;

namespace connector_daemon.Services.JobEventProcessing;

internal sealed class MonitoredRepositories : IEnumerable<AzureRepo>
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly HashSet<AzureRepo> _projects = new HashSet<AzureRepo>(new AzureRepoComparer());

    internal void Add(AzureRepo repo)
    {
        _semaphore.Wait();
        try
        {
            _projects.Add(repo);
        }
        finally
        {
            _semaphore.Release();    
        }
    }

    public IEnumerator<AzureRepo> GetEnumerator()
    {
        _semaphore.Wait();
        try
        {
            return _projects.ToList().GetEnumerator();
        }
        finally
        {
            _semaphore.Release();    
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class AzureRepoComparer : IEqualityComparer<AzureRepo>
    {
        public bool Equals(AzureRepo? x, AzureRepo? y)
            => x?.Id == y?.Id;

        public int GetHashCode(AzureRepo obj)
            => obj.Id.GetHashCode(StringComparison.Ordinal);
    }
}
