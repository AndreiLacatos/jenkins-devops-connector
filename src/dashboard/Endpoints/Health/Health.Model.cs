using System.Text.Json.Serialization;

namespace dashboard.Endpoints.Health;

internal static partial class Health
{
    internal sealed class HealthModel
    {
        [JsonPropertyName("healthy")]
        public bool Healthy { get; set; }

        [JsonPropertyName("failures")]
        public IEnumerable<string>? Failures { get; set; }
    }
}
