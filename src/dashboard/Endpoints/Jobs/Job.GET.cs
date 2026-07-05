using dashboard.Services;

namespace dashboard.Endpoints.Jobs;

internal static partial class Job
{
    internal static IEndpointRouteBuilder MapJobList(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/recent-jobs", HandleJobEvent);
        return builder;
    }

    private static async Task<IResult> HandleJobEvent(
        IJobListService jobListService,
        CancellationToken cancellationToken)
    {
        var jobs =  await jobListService.ListMostRecentJobsByRepositoriesAsync(cancellationToken);
        return Results.Ok(Map(jobs));
    }
}
