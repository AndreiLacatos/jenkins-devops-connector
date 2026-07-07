using System.Text.RegularExpressions;
using connector_daemon.AzureIntegration.Models;
using connector_daemon.Services.JobEventProcessing.CommentMetadata;

namespace connector_daemon.Services.JobEventProcessing.Extensions;

internal static partial class PullRequestThreadExtensions
{
    private const string ConnectorSystemCommentHeader = "<!-- Comment added by jenkins-devops-connector -->";
    private const string ThreadMetaDataTemplate = "<!-- jenkins-devops-connector-thread meta-data {0} -->";
    private static readonly Regex MetadataRegex = CommentMetaRegex();

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

        public AzureThread.AzureThreadComment TurnIntoConnectorSystemComment(ICommentMetadata? meta)
        {
            var metaSection = meta is null
                ? ConnectorSystemCommentHeader
                : string.Format(ThreadMetaDataTemplate, CommentMetadataSerializer.Encode(meta));
            return new AzureThread.AzureThreadComment
            {
                Id = comment.Id,
                Content = $"""
                          {ConnectorSystemCommentHeader}
                          {metaSection}
                          
                          
                          {comment.Content}
                          """,
                ParentId = comment.ParentId,
                PublishedAt = comment.PublishedAt,
            };
        }

        public ICommentMetadata? GetThreadMetadata()
        {
            if (!comment.IsConnectorSystemComment())
            {
                return null;
            }

            var match = MetadataRegex.Match(comment.Content);
            return match.Success ? CommentMetadataSerializer.Decode(match.Groups["meta"].Value) : null;
        }
    }

    [GeneratedRegex(
        @"<!--\s*jenkins-devops-connector-thread meta-data\s+(?<meta>.*?)\s*-->",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        "en-US")]
    private static partial Regex CommentMetaRegex();
}
