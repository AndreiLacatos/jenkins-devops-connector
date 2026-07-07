using dashboard.Integrations.Daemon;

namespace dashboard.Services;

internal sealed class JobQueueService : IJobQueueService
{
    private readonly IDaemonClient _client;

    public JobQueueService(IDaemonClient client)
    {
        _client = client;
    }

    public Task RequeueJobAsync(string name, int build, string commit, CancellationToken cancellationToken)
    {
        return _client.RequeueJobAsync(name, build, commit, cancellationToken);
    }
}
