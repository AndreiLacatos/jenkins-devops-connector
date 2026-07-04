namespace connector_daemon.AzureIntegration.Models;

internal sealed class AzureRepo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required AzureProject Project { get; init; }
}