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
                BranchJobs = jobSet.BranchJobs
                    .ToDictionary(
                        kvp => kvp.Key,
                        IDictionary<string, JobApiModel> (kvp) => kvp.Value
                            .ToDictionary(inner => inner.Key, inner => Map(inner.Value))),
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
