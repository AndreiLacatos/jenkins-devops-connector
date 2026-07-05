using System.Text.Json.Serialization;

namespace connector_sytem.Common.ApiModels.Jobs;

public sealed class MonitoredRepositories
{
    [JsonPropertyName("repositories")]
    public required IEnumerable<MonitoredRepository> Repositories { get; init; }
}
