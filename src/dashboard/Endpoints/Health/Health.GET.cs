using dashboard.Integrations.Daemon;

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
            if (health.Status.Equals("healthy", StringComparison.InvariantCultureIgnoreCase))
            {
                return Results.Ok(new HealthModel
                {
                    Healthy = true,
                });
            }
            else
            {
                var failures = new List<string>();
                if (!health.Entries["AZURE"].Status.Equals("healthy", StringComparison.InvariantCultureIgnoreCase))
                {
                    failures.Add($"Azure issue: {health.Entries["AZURE"].Description ?? "UNKNOWN"}");
                }
                if (!health.Entries["PERSISTENCE"].Status.Equals("healthy", StringComparison.InvariantCultureIgnoreCase))
                {
                    failures.Add($"Persistence issue: {health.Entries["PERSISTENCE"].Description ?? "UNKNOWN"}");
                }
                return Results.Ok(new HealthModel
                {
                    Healthy = false,
                    Failures = failures,
                });
            }
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
