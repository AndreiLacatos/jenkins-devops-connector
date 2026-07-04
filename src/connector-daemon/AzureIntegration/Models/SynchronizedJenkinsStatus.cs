namespace connector_daemon.AzureIntegration.Models;

internal sealed class SynchronizedJenkinsStatus
{
    public required AzureStateEnum? State { get; init; }
    public required DateTimeOffset CreationTime { get; init; }    
}
