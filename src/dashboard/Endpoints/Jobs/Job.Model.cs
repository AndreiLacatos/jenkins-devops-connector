using System.Text.Json.Serialization;

namespace dashboard.Endpoints.Jobs;

internal static partial class Job
{
    internal sealed class JobListResponseApiModel
    {
        [JsonPropertyName("jobs")]
        public required IEnumerable<RepositoryJobsApiModel> Jobs { get; set; }
    }

    internal sealed class RepositoryJobsApiModel
    {
        [JsonPropertyName("repository")]
        public required string RepositoryName { get; init; }

        [JsonPropertyName("branchJobs")]
        public required IDictionary<string, IDictionary<string, JobApiModel>> BranchJobs { get; init; }
    }
    
    internal sealed class JobApiModel
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("build")]
        public int Build { get; set; }

        [JsonPropertyName("commit")]
        public string Commit { get; set; }

        [JsonPropertyName("jobEvent")]
        public string JobEvent { get; set; }

        [JsonPropertyName("buildUrl")]
        public string? BuildUrl { get; set; }

        [JsonPropertyName("syncStatus")]
        public string SyncStatus { get; set; }

        [JsonPropertyName("registeredAt")]
        public string RegisteredAt { get; set; }

        [JsonPropertyName("enqueuedAt")]
        public string? EnqueuedAt { get; set; }

        [JsonPropertyName("finishedAt")]
        public string? FinishedAt { get; set; }
    }
}
