using System.Threading.Channels;
using connector_daemon.Persistence;
using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Services.JobEventProcessing;

internal sealed class JobEventProcessor : IJobEventProcessor
{
    private readonly ILogger<JobEventProcessor> _logger;
    private readonly IJobEventRepository _jobEventRepository;
    private readonly ChannelReader<JobEvent> _channelReader;

    public JobEventProcessor(
        ILogger<JobEventProcessor> logger,
        IJobEventRepository jobEventRepository,
        ChannelReader<JobEvent> channelReader)
    {
        _logger = logger;
        _jobEventRepository = jobEventRepository;
        _channelReader = channelReader;
    }

    public Task ProcessJobEventAsync(JobEvent job, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing event '{JobStatus}' for job {JobName} #{Build}",
            job.Status,
            job.Name,
            job.Build);

        return Task.CompletedTask;
    }
}
