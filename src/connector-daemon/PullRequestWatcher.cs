using connector_daemon.Services.PullRequestConsistency;

namespace connector_daemon;

internal sealed class PullRequestWatcher : BackgroundService
{
    private static readonly TimeSpan PullRequestRefreshFrequency = TimeSpan.FromMinutes(1);

    private readonly ILogger<PullRequestWatcher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;

    public PullRequestWatcher(
        ILogger<PullRequestWatcher> logger,
        IServiceProvider serviceProvider,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _timeProvider = timeProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var pullRequests = scope.ServiceProvider.GetRequiredService<IPullRequestStatusReconciler>();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(PullRequestRefreshFrequency, _timeProvider, stoppingToken);
                await pullRequests.ReconcileAsync(stoppingToken);
            }
            catch (Exception e)
            {
                
                if (e is TaskCanceledException or OperationCanceledException)
                {
                    return;
                }
                _logger.LogError(e, "Unhandled exception while watching pull requests");
            }
        }
    }
}
