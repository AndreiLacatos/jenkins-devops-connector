using connector_daemon.Persistence;

namespace connector_daemon.Services.JobEventProcessing;

internal sealed class JobEventStatusNormalizer : IJobEventStatusNormalizer
{
    private readonly ILogger<JobEventStatusNormalizer> _logger;
    private readonly IJobEventProcessingStatusRepository _repository;

    public JobEventStatusNormalizer(
        ILogger<JobEventStatusNormalizer> logger,
        IJobEventProcessingStatusRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task ResetStaleJobEventsAsync(CancellationToken cancellationToken)
    {
        var res = await Task.WhenAll(
            _repository.ListJobEventsAwaitingProcessingAsync(cancellationToken),
            _repository.ListJobEventsInProcessingAsync(cancellationToken));
        var brokenJobs = res[0].Union(res[1]);
        foreach (var job in brokenJobs)
        {
            await _repository.MarkJobEventPendingAsync(job, cancellationToken);
            _logger.LogInformation("Resetting processing status for broken event '{JobStatus}' for job {JobName} #{Build}",
                job.Status,
                job.Name,
                job.Build);
        }
    }
}
