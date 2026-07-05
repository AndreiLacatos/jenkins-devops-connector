using System.Text.Json.Serialization;

namespace connector_sytem.Common.ApiModels.Jobs;

public sealed class SyncJob
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("build")]
    public required int Build { get; init; }

    [JsonPropertyName("gitUrl")]
    public required string GitUrl { get; init; }

    [JsonPropertyName("commit")]
    public required string Commit { get; init; }

    [JsonPropertyName("branch")]
    public required string Branch { get; init; }

    [JsonPropertyName("jenkinsStatus")]
    public required string JenkinsStatus { get; init; }

    [JsonPropertyName("buildUrl")]
    public required string? BuildUrl { get; init; }

    [JsonPropertyName("registeredAt")]
    public required string RegisteredAt { get; set; }

    [JsonPropertyName("syncStatus")]
    public required string SyncStatus { get; set; }

    [JsonPropertyName("enqueuedAt")]
    public required string? EnqueuedAt { get; set; }

    [JsonPropertyName("finishedAt")]
    public required string? FinishedAt { get; set; }
}
