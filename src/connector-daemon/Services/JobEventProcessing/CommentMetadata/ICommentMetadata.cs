using System.Text.Json.Serialization;

namespace connector_daemon.Services.JobEventProcessing.CommentMetadata;

internal interface ICommentMetadata
{
    [JsonIgnore]
    int Version { get; }
}
