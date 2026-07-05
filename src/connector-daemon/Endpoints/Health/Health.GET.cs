using Microsoft.Extensions.Diagnostics.HealthChecks;
using HealthModel = connector_sytem.Common.ApiModels.Health;

namespace connector_daemon.Endpoints.Health;

internal static class Health
{
    internal static IEndpointRouteBuilder MapHealthCheck(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/health", HandleHealth);
        return builder;
    }

    private static async Task<IResult> HandleHealth(
        HealthCheckService healthCheckService,
        CancellationToken cancellationToken)
    {
        var report = await healthCheckService.CheckHealthAsync(cancellationToken);

        var response = new HealthModel
        {
            Healthy = report.Status == HealthStatus.Healthy,
            Failures = report.Entries
                .Where(e => e.Value.Status != HealthStatus.Healthy)
                .Select(e => $"{e.Key} issues: {e.Value.Description ?? "UNKNOWN"}")
                .ToArray(),
        };

        return Results.Json(response);
    }
}