using connector_daemon.Services.EventRegistration;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace connector_daemon.Endpoints.Jobs;

internal static partial class JobEvent
{
    internal static IEndpointRouteBuilder MapJobEventWebhook(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/v1/job-events", HandleJobEvent);
        return builder;
    }

    private static async Task<IResult> HandleJobEvent(
        [FromBody] JobEventApiModel jobEventApi,
        [FromServices] IValidator<JobEventApiModel> validator,
        [FromServices] IJobEventRegistrar registrar,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(jobEventApi, cancellationToken);
        if (!validationResult.IsValid)
        {
            var issues = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(issues);
        }

        await registrar.RegisterJobEventAsync(JobEventMapper.Map(jobEventApi), cancellationToken);
        return Results.Accepted();
    }
}
