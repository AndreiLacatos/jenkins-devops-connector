using connector_daemon.Persistence;
using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Services.EventRegistration;

internal sealed class JobEventRegistrar : IJobEventRegistrar
{
    private readonly ILogger<JobEventRegistrar> _logger;
    private readonly IJobEventRepository _jobEventRepository;
    private readonly TimeProvider _timeProvider;

    public JobEventRegistrar(
        ILogger<JobEventRegistrar> logger,
        IJobEventRepository jobEventRepository,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _jobEventRepository = jobEventRepository;
        _timeProvider = timeProvider;
    }

    public async Task RegisterJobEventAsync(JobEventRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Registering event '{JobStatus}' for job {JobName} #{Build}",
            request.Status,
            request.Name,
            request.Build);

        var buildEvents = await _jobEventRepository.ListJobEventsAsync(
            new JobEventListFilter
            {
                JobName = request.Name,
                Build = request.Build,
                Commit = request.Commit,
            },
            cancellationToken);

        var existingEvent = buildEvents.FirstOrDefault(e => e.Status == request.Status);
        if (existingEvent is not null)
        {
            // An event with the same status has already been recorded for this build.
            // This is unexpected (duplicate webhook or repeated processing), so ignore it.
            _logger.LogWarning(
                "Status '{JobStatus}' for job {JobName} #{Build} already exists (registered at {EventTimestamp})",
                existingEvent.Status,
                existingEvent.Name,
                existingEvent.Build,
                existingEvent.RegisteredAt);
            return;
        }

        await _jobEventRepository.SaveJobEventAsync(
            JobEvent.FromRequest(request, _timeProvider.GetUtcNow()),
            cancellationToken);
    }
}
