using connector_daemon.AzureIntegration.Models;
using connector_daemon.Services.EventRegistration.Models;
using connector_daemon.Services.JobEventProcessing.Extensions;

namespace connector_daemon.Services.JobEventProcessing.CommentingStrategy;

/// <summary>
/// Uses a single discussion thread for all job events
/// </summary>
internal sealed class SingleThreadStrategy : ICommentingStrategy
{
    public AzureThread? GetTargetThread(IEnumerable<AzureThread> existingThreads, JobEvent job)
    {
        return existingThreads.GetJenkinsPipelineThread();
    }
}
