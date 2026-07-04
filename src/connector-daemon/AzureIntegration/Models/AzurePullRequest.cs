namespace connector_daemon.AzureIntegration.Models;

internal sealed class AzurePullRequest
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required SynchronizedJenkinsStatus? LatestJenkinsStatus { get; init; }
}
