using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace connector_daemon.Services.JobEventProcessing.CommentMetadata;

internal static class CommentMetadataSerializer
{
    public static string Encode(ICommentMetadata meta)
    {
        var envelope = new MetadataEnvelope
        {
            Version = meta.Version,
            Payload = JsonSerializer.Serialize(meta, meta.GetType()),
        };
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope)));
    }

    public static ICommentMetadata? Decode(string metaStr)
    {
        metaStr = metaStr.Trim();
        if (string.IsNullOrWhiteSpace(metaStr))
        {
            return null;
        }

        var metaJson = Encoding.UTF8.GetString(Convert.FromBase64String(metaStr));
        var envelope = JsonSerializer.Deserialize<MetadataEnvelope>(metaJson);
        if (envelope?.Version is null || envelope.Payload is null)
        {
            return null;
        }

        return envelope.Version.Value switch
        {
            1 => JsonSerializer.Deserialize<CommentMetadataV1>(envelope.Payload),
            _ => null,
        };
    }

    private sealed class MetadataEnvelope
    {
        [JsonPropertyName("__version")]
        public required int? Version { get; init; }

        [JsonPropertyName("payload")]
        public string? Payload { get; set; }
    }
}
