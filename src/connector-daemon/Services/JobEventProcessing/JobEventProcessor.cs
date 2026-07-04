using connector_daemon.AzureIntegration;
using connector_daemon.AzureIntegration.Models;
using connector_daemon.Persistence;
using connector_daemon.Services.EventRegistration.Models;

namespace connector_daemon.Services.JobEventProcessing;

internal sealed class JobEventProcessor : IJobEventProcessor
{
    private readonly ILogger<JobEventProcessor> _logger;
    private readonly IJobEventProcessingStatusRepository _repository;
    private readonly IAzureClient _azureClient;

    public JobEventProcessor(
        ILogger<JobEventProcessor> logger,
        IJobEventProcessingStatusRepository repository,
        IAzureClient azureClient)
    {
        _logger = logger;
        _repository = repository;
        _azureClient = azureClient;
    }

    public async Task ProcessJobEventAsync(JobEvent job, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Processing event '{JobStatus}' for job {JobName} #{Build}",
            job.Status,
            job.Name,
            job.Build);

        // sanity check, ensure nothing else has already processed this job
        var processedJobs = await _repository.ListProcessedJobEvents(
            new JobEventListFilter
            {
                Build = job.Build,
                JobName = job.Name,
                Commit = job.Commit,
            },
            cancellationToken);
        if (processedJobs.Any(j => j.Status == job.Status))
        {
            _logger.LogDebug("Event '{JobStatus}' for job {JobName} #{Build} has already been processed. Skipping",
                job.Status,
                job.Name,
                job.Build);
            return;
        }

        // mark the job as being actively processing
        await _repository.MarkJobEventProcessingAsync(job, cancellationToken);

        try
        {
            var azureRepo = await _azureClient.GetRepositoryAsync(job.GitUrl, cancellationToken);
            var azureCommit = await _azureClient.GetCommitAsync(azureRepo, job.Commit, cancellationToken);
            var azurePrs = await _azureClient.ListAssociatedActivePullRequestsAsync(azureRepo, azureCommit, cancellationToken);

            var commitHasTerminalState = azureCommit.LatestJenkinsStatus?.State
                is AzureStateEnum.Succeeded or AzureStateEnum.Failed;
            if (job.Status == JobStatus.Started && commitHasTerminalState &&
                (job.RegisteredAt - azureCommit.LatestJenkinsStatus!.CreationTime).Duration() < TimeSpan.FromMinutes(1))
            {
                // Azure already reports a terminal commit status (success/failure) very close to the
                // Jenkins pipeline start event. To avoid issues caused by out-of-order event processing,
                // we skip setting a "pending" status. Otherwise, a late-arriving start event could
                // overwrite or conflict with an already finalized status. (e.g. Pipeline failed really fast,
                // 300ms after start, we've already synchronized the failure with Azure, and we are processing
                // the start event late (can happen due to the high degree of parallelism, random network delays,
                // etc.))
                _logger.LogWarning(
                    "Azure commit already reflects '{AzureCommitState}', that was set really close to a Jenkins pipeline" +
                    "start event (Azure commit state set at {AzureStateTime}, Jenkins event received at {JenkinsEventReceived})." +
                    " Skipping this sync to avoid potentially corrupting Azure state.",
                    azureCommit.LatestJenkinsStatus.State,
                    azureCommit.LatestJenkinsStatus.CreationTime,
                    job.RegisteredAt);
            }
            else
            {
                // set commit status
                await _azureClient.SetCommitStatusAsync(azureRepo, azureCommit, job, cancellationToken);
            }

            foreach (var azurePr in azurePrs)
            {
                var prHasTerminalState = azurePr.LatestJenkinsStatus?.State
                    is AzureStateEnum.Succeeded or AzureStateEnum.Failed;
                if (job.Status == JobStatus.Started && prHasTerminalState &&
                    (job.RegisteredAt - azurePr.LatestJenkinsStatus!.CreationTime).Duration() < TimeSpan.FromMinutes(1))
                {
                    // Azure already reports a terminal PR status (success/failure) very close to the
                    // Jenkins pipeline start event. To avoid issues caused by out-of-order event processing,
                    // we skip setting a "pending" status. Otherwise, a late-arriving start event could
                    // overwrite or conflict with an already finalized status. (e.g. Pipeline failed really fast,
                    // 300ms after start, we've already synchronized the failure with Azure, and we are processing
                    // the start event late (can happen due to the high degree of parallelism, random network delays,
                    // etc.))
                    _logger.LogWarning(
                        "Azure PR '{AzurePr}' already reflects '{AzurePrState}', that was set really close to a Jenkins pipeline" +
                        " start event (Azure PR state set at {AzureStateTime}, Jenkins event received at {JenkinsEventReceived})." +
                        " Skipping this sync to avoid potentially corrupting Azure state.",
                        azurePr.Title,
                        azurePr.LatestJenkinsStatus.State,
                        azurePr.LatestJenkinsStatus.CreationTime,
                        job.RegisteredAt);
                }
                else
                {
                    // set PR status
                    await _azureClient.SetPrStatusAsync(azureRepo, azurePr, job, cancellationToken);
                }
            }

            // by some borderline miracle, nothing exploded (or at least nothing we’ve noticed yet...)
            // Finally, mark the Jenkins job event as completed.
            await _repository.MarkJobEventCompletedAsync(job, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to process event '{JobStatus}' for job {JobName} #{Build}.",
                job.Status,
                job.Name,
                job.Build);
            await _repository.MarkJobEventFailedAsync(job, cancellationToken);
        }
    }
}
