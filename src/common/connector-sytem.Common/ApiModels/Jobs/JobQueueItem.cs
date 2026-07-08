using System.Text.Json.Serialization;

namespace connector_sytem.Common.ApiModels.Jobs;

public sealed class JobQueueItem
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("build")]
    public int? Build { get; set; }

    [JsonPropertyName("commit")]
    public string? Commit { get; set; }
}