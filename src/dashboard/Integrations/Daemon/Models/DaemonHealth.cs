using System.Text.Json.Serialization;

namespace dashboard.Integrations.Daemon.Models;

internal sealed class DaemonHealth
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("entries")]
    public required Dictionary<string, HealthEntry> Entries { get; init; }
}

internal sealed class HealthEntry
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
