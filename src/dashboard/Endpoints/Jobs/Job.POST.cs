using dashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace dashboard.Endpoints.Jobs;

internal static partial class Job
{
    internal static IEndpointRouteBuilder MapRequeueJob(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/jobs/{name}/{build:int}/{commit}/requeue", HandleRequeue);
        return builder;
    }

    private static async Task<IResult> HandleRequeue(
        [FromRoute(Name = "name")] string name,
        [FromRoute(Name = "build")] int build,
        [FromRoute(Name = "commit")] string commit,
        IJobQueueService service,
        CancellationToken cancellationToken)
    {
        await service.RequeueJobAsync(name, build, commit, cancellationToken);
        return Results.Accepted();
    }
}
