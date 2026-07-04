namespace connector_daemon.AzureIntegration.Models;

internal sealed class AzureCommit
{
    public required string Id { get; init; }
    public required SynchronizedJenkinsStatus? LatestJenkinsStatus { get; init; }
}
