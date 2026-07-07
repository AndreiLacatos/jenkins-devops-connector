using connector_daemon.AzureIntegration.Models;
using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Services.JobEventProcessing.CommentingStrategy;

internal interface ICommentingStrategy
{
    AzureThread? GetTargetThread(IEnumerable<AzureThread> existingThreads, JobEvent job);
}
