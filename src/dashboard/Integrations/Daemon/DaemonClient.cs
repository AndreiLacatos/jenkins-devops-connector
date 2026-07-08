using System.Text.Json;
using connector_sytem.Common.ApiModels;
using connector_sytem.Common.ApiModels.Jobs;

namespace dashboard.Integrations.Daemon;

internal sealed class DaemonClient : IDaemonClient
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DaemonClient(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<MonitoredRepositories> ListRepositoriesAsync(CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(nameof(DaemonClient));
        var response = await client.GetAsync("api/system/jobs/repositories", cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<MonitoredRepositories>(payload)!;
    }

    public async Task<RepositoryJobs> ListRepositoryJobsAsync(string repository, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(nameof(DaemonClient));
        var response = await client.GetAsync($"/api/system/jobs/{repository}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<RepositoryJobs>(payload)!;
    }

    public async Task<Health> GetDaemonHealthAsync(CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(nameof(DaemonClient));
        var response = await client.GetAsync($"/health", cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<Health>(payload)!;
    }

    public async Task RequeueJobAsync(string name, int build, string commit, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(nameof(DaemonClient));
        var response = await client.PostAsync(
            "/api/system/jobs/requeue",
            JsonContent.Create(new JobQueueItem
            {
                Name = name,
                Build = build,
                Commit = commit,
            }),
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
