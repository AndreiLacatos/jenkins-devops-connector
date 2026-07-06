using connector_daemon.Services.PullRequestConsistency;
using Microsoft.Extensions.Options;

namespace connector_daemon;

internal sealed class PullRequestWatcher : BackgroundService
{
    private readonly ILogger<PullRequestWatcher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _pullRequestRefreshFrequency;

    public PullRequestWatcher(
        ILogger<PullRequestWatcher> logger,
        IServiceProvider serviceProvider,
        TimeProvider timeProvider,
        IOptions<PullRequestWatcherOptions> options)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _timeProvider = timeProvider;
        _pullRequestRefreshFrequency = TimeSpan.FromMinutes(options.Value.PullRequestRefreshIntervalMinutes ?? 5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var pullRequests = scope.ServiceProvider.GetRequiredService<IPullRequestStatusReconciler>();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_pullRequestRefreshFrequency, _timeProvider, stoppingToken);
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
