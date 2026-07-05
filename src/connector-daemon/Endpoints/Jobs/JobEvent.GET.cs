using connector_daemon.Services.EventRegistration.Models;
using connector_daemon.Services.JobEventProcessing;
using connector_daemon.Services.JobEventProcessing.Models;
using connector_sytem.Common.ApiModels.Jobs;
using Microsoft.AspNetCore.Mvc;
using MonitoredRepositoriesModel = connector_sytem.Common.ApiModels.Jobs.MonitoredRepositories;
using MonitoredRepositoriesObject = connector_daemon.Services.JobEventProcessing.MonitoredRepositories;

namespace connector_daemon.Endpoints.Jobs;

internal static class JobEventGET
{
    internal static IEndpointRouteBuilder MapJobEventQueries(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/system/jobs/repositories", HandleListRepositories);
        builder.MapGet("/api/system/jobs/{repository}", HandleListRepositoryJobs);
        return builder;
    }

    private static Task<IResult> HandleListRepositories(MonitoredRepositoriesObject monitoredRepositories)
    {
        return Task.FromResult(Results.Ok(new MonitoredRepositoriesModel
        {
            Repositories = monitoredRepositories.Select(repo => new MonitoredRepository { Name = repo.Name }).ToList(),
        }));
    }

    private static async Task<IResult> HandleListRepositoryJobs(
        [FromRoute(Name = "repository")] string repository,
        [FromServices] IJobEventProcessingQueryService queryService,
        CancellationToken cancellationToken)
    {
        var jobs = await queryService.ListRepositoryJobsByBranchAsync(repository, cancellationToken);
        return Results.Ok(new RepositoryJobs
        {
            RepositoryName = repository,
            BranchJobs = jobs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(Map)),
        });
    }

    private static SyncJob Map(Job job)
    {
        return new SyncJob
        {
            Branch =  job.Branch,
            Name =  job.Name,
            Build =  job.Build,
            BuildUrl =   job.BuildUrl,
            Commit =  job.Commit,
            EnqueuedAt = job.EnqueuedAt?.ToString("O"),
            FinishedAt = job.FinishedAt?.ToString("O"),
            GitUrl =  job.GitUrl,
            RegisteredAt = job.RegisteredAt.ToString("O"),
            JenkinsStatus = job.Status switch
            {
                JobStatus.Started => "started",
                JobStatus.Succeeded => "succeeded",
                JobStatus.Failed => "failed",
                _ => "aborted",
            },
            SyncStatus = job.SyncStatus switch
            {
                SyncStatus.Pending => "pending",
                SyncStatus.Enqueued => "enqueued",
                SyncStatus.Processing => "processing",
                SyncStatus.Completed => "completed",
                _ => "failed",
            },
        };
    }
}
