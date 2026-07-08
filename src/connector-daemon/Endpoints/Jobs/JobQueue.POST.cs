using connector_daemon.Services.EventRegistration;
using connector_daemon.Services.EventRegistration.Models;
using connector_sytem.Common.ApiModels.Jobs;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace connector_daemon.Endpoints.Jobs;

internal static partial class JobQueue
{
    internal static IEndpointRouteBuilder MapJobEventManualRequeue(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/system/jobs/requeue", HandleManualRequeue);
        return builder;
    }

    private static async Task<IResult> HandleManualRequeue(
        [FromBody] JobQueueItem queueItem,
        [FromServices] IValidator<JobQueueItem> validator,
        [FromServices] IJobEventRegistrar registrar,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(queueItem, cancellationToken);
        if (!validationResult.IsValid)
        {
            var issues = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(issues);
        }

        await registrar.RequeueJobEventAsync(
            new JobEventRequeueRequest
            {
                Name = queueItem.Name!,
                Build = queueItem.Build!.Value,
                Commit = queueItem.Commit!,
            },
            cancellationToken);
        return Results.Accepted();
    }
}
