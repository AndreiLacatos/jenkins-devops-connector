using connector_daemon.AzureIntegration;
using connector_daemon.AzureIntegration.Models;
using connector_daemon.Services.EventRegistration.Models;
using connector_daemon.Services.JobEventProcessing.CommentMetadata;
using connector_daemon.Services.JobEventProcessing.Extensions;

namespace connector_daemon.Services.JobEventProcessing;

internal sealed class PullRequestCommenter
{
    private readonly IAzureClient _azureClient;

    public PullRequestCommenter(IAzureClient azureClient)
    {
        _azureClient = azureClient;
    }

    internal async Task AddPipelineStatusCommentAsync(
        AzureRepo repo,
        AzurePullRequest pullRequest,
        JobEvent job,
        CancellationToken cancellationToken)
    {
        if (job.Status is JenkinsPipelineStatus.Failed or JenkinsPipelineStatus.Aborted)
        {
            // Jenkins pipeline failed, add a comment
            var comment = new AzureThread.AzureThreadComment
            {
                Content = string.IsNullOrWhiteSpace(job.BuildUrl)
                    ? $"Jenkins build #{job.Build} failed."
                    : $"⛔ Jenkins job [{Uri.UnescapeDataString(job.Name)} #{job.Build}]({job.BuildUrl}) failed.",
            }.TurnIntoConnectorSystemComment(new CommentMetadataV1 { JenkinsJob = job.Name, JenkinsJobSuccessful = false });
            var threads = await _azureClient.ListPullRequestThreadsAsync(repo, pullRequest, cancellationToken);
            var thread = threads.GetJenkinsPipelineThread();
            if (thread is null)
            {
                // there is no thread regarding Jenkins pipeline status yet, create it with the comment
                await _azureClient.AddPullRequestThreadAsync(repo, pullRequest, comment, cancellationToken);
            }
            else
            {
                // add another comment to the existing thread
                comment.ParentId = thread.GetRootComment()?.Id;
                await _azureClient.AddPullRequestCommentAsync(repo, pullRequest, thread, comment, cancellationToken);
            }
        }

        if (job.Status == JenkinsPipelineStatus.Succeeded)
        {
            // Jenkins pipeline succeeded, if there is a comment on the PR regarding the failure,
            // respond to it and resolve thread (unless the most recent comment already indicates success)
            var threads = await _azureClient.ListPullRequestThreadsAsync(repo, pullRequest, cancellationToken);
            var thread = threads.GetJenkinsPipelineThread();
            if (thread is not null)
            {
                var meta = thread.GetMostRecentComment()?.GetThreadMetadata();
                var shouldResolveThread = meta switch
                {
                    // thread needs resolution if the most recent comment indicates failure
                    CommentMetadataV1 v1 => !v1.JenkinsJobSuccessful,

                    // resolve the thread if there is no metadata to ensure we do not leave any threads hanging 
                    _ => true,
                };
                if (shouldResolveThread)
                {
                    var comment = new AzureThread.AzureThreadComment
                    {
                        Content = string.IsNullOrWhiteSpace(job.BuildUrl)
                            ? $"Jenkins build #{job.Build} succeeded."
                            : $"✅ Jenkins job [{Uri.UnescapeDataString(job.Name)} #{job.Build}]({job.BuildUrl}) succeeded.",
                        ParentId = thread.GetRootComment()?.Id,
                    }.TurnIntoConnectorSystemComment(new CommentMetadataV1 { JenkinsJob = job.Name, JenkinsJobSuccessful = true });
                    await _azureClient.ResolvePullRequestThreadAsync(repo, pullRequest, thread, comment, cancellationToken);
                }
            }
        }
    }
}
