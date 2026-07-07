using connector_daemon.AzureIntegration.Models;
using connector_daemon.Services.EventRegistration.Models;
using connector_daemon.Services.JobEventProcessing.CommentMetadata;
using connector_daemon.Services.JobEventProcessing.Extensions;

namespace connector_daemon.Services.JobEventProcessing.CommentingStrategy;

/// <summary>
/// Uses a separate discussion thread for each job
/// </summary>
internal sealed class JobThreadStrategy : ICommentingStrategy
{
    public AzureThread? GetTargetThread(IEnumerable<AzureThread> existingThreads, JobEvent job)
    {
        var systemThreads = existingThreads.GetJenkinsPipelineThreads();
        foreach (var systemThread in systemThreads)
        {
            var meta = systemThread.GetMostRecentComment()?.GetThreadMetadata();
            switch (meta)
            {
                case CommentMetadataV1 v1:
                    if (v1.JenkinsJob.Trim().Equals(job.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        // jackpot, this thread matches this job
                        return systemThread;
                    }
                    break;
                default: continue;
            }
        }

        return null;
    }
}
