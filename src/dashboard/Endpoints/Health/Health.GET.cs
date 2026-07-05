using dashboard.Integrations.Daemon;
using HealthModel = connector_sytem.Common.ApiModels.Health;

namespace dashboard.Endpoints.Health;

internal static partial class Health
{
    internal static IEndpointRouteBuilder MapDaemonHealth(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/daemon-health", HandleHealth);
        return builder;
    }

    private static async Task<IResult> HandleHealth(
        IDaemonClient daemonClient,
        CancellationToken cancellationToken)
    {
        try
        {
            var health = await daemonClient.GetDaemonHealthAsync(cancellationToken);
            return Results.Ok(health);
        }
        catch (Exception e)
        {
            return Results.Ok(new HealthModel
            {
                Healthy = false,
                Failures =
                [
                    $"Service unreachable: {e.Message}",
                ],
            });
        }
    }
}
