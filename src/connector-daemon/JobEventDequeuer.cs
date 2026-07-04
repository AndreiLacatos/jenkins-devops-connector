using System.Threading.Channels;
using connector_daemon.Services.EventRegistration.Models;
using connector_daemon.Services.JobEventProcessing;

namespace connector_daemon;

internal sealed class JobEventDequeuer : BackgroundService
{
    private readonly ILogger<JobEventDequeuer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ChannelReader<JobEvent> _channelReader;

    public JobEventDequeuer(
        ILogger<JobEventDequeuer> logger,
        IServiceProvider serviceProvider,
        ChannelReader<JobEvent> channelReader)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _channelReader = channelReader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var job in _channelReader.ReadAllAsync(stoppingToken))
                {
                    _ = ProcessJobAsync(job, stoppingToken);
                }
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException or OperationCanceledException)
                {
                    return;
                }
                _logger.LogError(e, "Unhandled exception while dequeuing job events");
            }
        }
    }

    private async Task ProcessJobAsync(JobEvent job, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var processor = scope.ServiceProvider.GetRequiredService<IJobEventProcessor>();
            await processor.ProcessJobEventAsync(job, cancellationToken);
        }
        catch (Exception e)
        {
            if (e is TaskCanceledException or OperationCanceledException)
            {
                return;
            }
            _logger.LogError(e, "Error processing event '{JobStatus}' for job {JobName} #{Build}",
                job.Status,
                job.Name,
                job.Build);
        }
    }
}
