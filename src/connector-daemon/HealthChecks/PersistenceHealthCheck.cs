using connector_daemon.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace connector_daemon.HealthChecks;

internal sealed class PersistenceHealthCheck : IHealthCheck
{
    private readonly ILogger<PersistenceHealthCheck> _logger;
    private readonly IJobEventRepository _repository;

    public PersistenceHealthCheck(
        ILogger<PersistenceHealthCheck> logger,
        IJobEventRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await _repository.ListJobEventsAsync(
                new JobEventListFilter
                {
                    Commit = "fake-commit",
                },
                cancellationToken);

            // if query succeeds, all good
            return HealthCheckResult.Healthy($"Persistence OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Persistence health check failed");
            return HealthCheckResult.Unhealthy("Persistence health check failed", ex);
        }
    }
}