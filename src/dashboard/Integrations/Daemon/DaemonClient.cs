using System.Text.Json;
using connector_sytem.Common.ApiModels.Jobs;
using dashboard.Integrations.Daemon.Models;

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

    public async Task<DaemonHealth> GetDaemonHealthAsync(CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient(nameof(DaemonClient));
        var response = await client.GetAsync($"/health", cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<DaemonHealth>(payload)!;
    }
}
