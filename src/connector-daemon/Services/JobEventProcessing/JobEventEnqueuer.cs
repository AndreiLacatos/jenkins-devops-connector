using System.Threading.Channels;
using connector_daemon.Persistence;
using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Services.JobEventProcessing;

internal sealed class JobEventEnqueuer : IJobEventEnqueuer
{
    private readonly IJobEventProcessingStatusRepository _jobEventRepository;
    private readonly ChannelWriter<JobEvent> _channelWriter;

    public JobEventEnqueuer(
        IJobEventProcessingStatusRepository jobEventRepository,
        ChannelWriter<JobEvent> channelWriter)
    {
        _jobEventRepository = jobEventRepository;
        _channelWriter = channelWriter;
    }

    public async Task EnqueuePendingJobs(CancellationToken cancellationToken)
    {
        var jobs = await _jobEventRepository.ListJobEventsAwaitingProcessingAsync(cancellationToken);
        foreach (var job in jobs)
        {
            await _channelWriter.WriteAsync(job, cancellationToken);
            await _jobEventRepository.MarkJobEventEnqueuedAsync(job, cancellationToken);
        }
    }
}
