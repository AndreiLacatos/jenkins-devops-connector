using connector_daemon.AzureIntegration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

internal sealed class AzureHealthCheck : IHealthCheck
{
    private readonly ILogger<AzureHealthCheck> _logger;
    private readonly IAzureClient _azureClient;

    public AzureHealthCheck(
        ILogger<AzureHealthCheck> logger,
        IAzureClient azureClient)
    {
        _logger = logger;
        _azureClient = azureClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var repos = await _azureClient.ListVisibleRepositories(cancellationToken: cancellationToken);
            var repoCount = repos.Count();
            if (repoCount == 0)
            {
                return HealthCheckResult.Unhealthy("No Azure repositories are available.");
            }

            return HealthCheckResult.Healthy($"Azure OK ({repoCount} repositories available)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure health check failed");
            return HealthCheckResult.Unhealthy("Azure connection failed", ex);
        }
    }
}
