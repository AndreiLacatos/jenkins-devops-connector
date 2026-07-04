using connector_daemon.Services.JobEventProcessing;

namespace connector_daemon;

internal sealed class JobEventWatcher : BackgroundService
{
    private static readonly TimeSpan ScanFrequency = TimeSpan.FromSeconds(15);
    private readonly ILogger<JobEventWatcher> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeProvider _timeProvider;

    public JobEventWatcher(
        ILogger<JobEventWatcher> logger,
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
        var enqueuer = scope.ServiceProvider.GetRequiredService<IJobEventEnqueuer>();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await enqueuer.EnqueuePendingJobs(stoppingToken);
                await Task.Delay(ScanFrequency, _timeProvider, stoppingToken);
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException or OperationCanceledException)
                {
                    return;
                }
                _logger.LogError(e, "Unhandled exception while watching job events");
            }
        }
    }
}
