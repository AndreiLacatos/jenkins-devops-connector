using System.Text.Json.Serialization;

namespace connector_sytem.Common.ApiModels.Jobs;

public sealed class RepositoryJobs
{
    [JsonPropertyName("repository")]
    public required string RepositoryName { get; init; }

    [JsonPropertyName("jobs")]
    public required IDictionary<string, IEnumerable<SyncJob>> BranchJobs { get; init; }
}
