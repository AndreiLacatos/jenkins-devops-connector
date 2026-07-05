using System.Text.Json.Serialization;

namespace connector_sytem.Common.ApiModels.Jobs;

public sealed class MonitoredRepository
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}
