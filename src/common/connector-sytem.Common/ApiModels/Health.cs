using System.Text.Json.Serialization;

namespace connector_sytem.Common.ApiModels;
public sealed class Health
{
    [JsonPropertyName("healthy")]
    public bool Healthy { get; init; }

    [JsonPropertyName("failures")]
    public IEnumerable<string>? Failures { get; init; }
}