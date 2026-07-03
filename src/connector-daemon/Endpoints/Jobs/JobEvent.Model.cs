using System.Text.Json.Serialization;

namespace connector_daemon.Endpoints.Jobs;

internal static partial class JobEvent
{
    internal sealed class JobEventApiModel
    {
        [JsonPropertyName("job")]
        public string? Name { get; init; }

        [JsonPropertyName("build")]
        public int? Build { get; init; }

        [JsonPropertyName("commit")]
        public string? Commit { get; init; }
        
        [JsonPropertyName("status")]
        public string? Status { get; init; }
        
        [JsonPropertyName("url")]
        public string? Url { get; init; }
    }
}
