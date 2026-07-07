using connector_daemon.Services.EventRegistration;
using connector_daemon.Services.EventRegistration.Models;
using Microsoft.AspNetCore.Mvc;

namespace connector_daemon.Endpoints.Jobs;

internal static class JobEventPUT
{
    internal static IEndpointRouteBuilder MapJobEventManualRequeue(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/system/jobs/{name}/{build:int}/{commit}/requeue", HandleManualRequeue);
        return builder;
    }

    private static async Task<IResult> HandleManualRequeue(
        [FromRoute(Name = "name")] string name,
        [FromRoute(Name = "build")] int build,
        [FromRoute(Name = "commit")] string commit,
        [FromServices] IJobEventRegistrar registrar,
        CancellationToken cancellationToken)
    {
        await registrar.RequeueJobEventAsync(
            new JobEventRequeueRequest
            {
                Name = name,
                Build = build,
                Commit = commit
            },
            cancellationToken);
        return Results.Accepted();
    }
}
