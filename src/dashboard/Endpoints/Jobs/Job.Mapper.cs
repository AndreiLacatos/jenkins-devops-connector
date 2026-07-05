using dashboard.Services.Models;
using JobModel = dashboard.Services.Models.Job;

namespace dashboard.Endpoints.Jobs;

internal static partial class Job
{
    private static JobListResponseApiModel Map(IEnumerable<RepositoryJobs> jobs)
    {
        return new JobListResponseApiModel
        {
            Jobs = jobs.Select(jobSet => new RepositoryJobsApiModel
            {
                RepositoryName = jobSet.RepositoryName,
                BranchJobs = jobSet.BranchJobs.ToDictionary(kvp => kvp.Key, kvp => Map(kvp.Value)),
            }),
        };
    }

    private static JobApiModel Map(JobModel job)
    {
        return new JobApiModel
        {
            Build = job.Build,
            Commit = job.Commit,
            BuildUrl = job.BuildUrl,
            EnqueuedAt = job.EnqueuedAt,
            FinishedAt = job.FinishedAt,
            JobEvent = job.JobEvent,
            Name = job.Name,
            RegisteredAt = job.RegisteredAt,
            SyncStatus = job.SyncStatus,
        };
    }
}
