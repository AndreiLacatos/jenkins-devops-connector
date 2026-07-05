namespace connector_daemon.Services.PullRequestConsistency;

public interface IPullRequestStatusReconciler
{
    Task ReconcileAsync(CancellationToken cancellationToken);
}
