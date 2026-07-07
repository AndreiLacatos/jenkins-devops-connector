using System.Text.Json.Serialization;

namespace connector_daemon.Services.JobEventProcessing.CommentMetadata;

internal sealed class CommentMetadataV1 : ICommentMetadata
{
    public int Version => 1;

    [JsonPropertyName("job")]
    public required string JenkinsJob { get; set; }

    [JsonPropertyName("successful")]
    public required bool JenkinsJobSuccessful { get; set; }
}
