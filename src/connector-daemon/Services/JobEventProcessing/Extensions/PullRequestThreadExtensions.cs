using connector_daemon.AzureIntegration.Models;

namespace connector_daemon.Services.JobEventProcessing.Extensions;

internal static class PullRequestThreadExtensions
{
    private const string ConnectorSystemCommentHeader = "<!-- Comment added by jenkins-devops-connector -->";

    extension(IEnumerable<AzureThread> threads)
    {
        public AzureThread? GetJenkinsPipelineThread()
        {
            return threads.FirstOrDefault(
                thread => thread.Comments.Any(comment => comment.IsConnectorSystemComment()));
        }
    }

    extension(AzureThread thread)
    {
        public AzureThread.AzureThreadComment? GetRootComment()
        {
            return thread.Comments.MinBy(c => c.PublishedAt);
        }
    }

    extension(AzureThread.AzureThreadComment comment)
    {
        public bool IsConnectorSystemComment()
        {
            return comment.Content.Contains(ConnectorSystemCommentHeader, StringComparison.InvariantCultureIgnoreCase);
        }

        public AzureThread.AzureThreadComment TurnIntoConnectorSystemComment()
        {
            return new AzureThread.AzureThreadComment
            {
                Id = comment.Id,
                Content = $"""
                          {ConnectorSystemCommentHeader}
                          
                          
                          {comment.Content}
                          """,
                ParentId = comment.ParentId,
                PublishedAt = comment.PublishedAt,
            };
        }
    }
}
