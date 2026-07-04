using connector_daemon.Services.EventRegistration.Models;
namespace connector_daemon.Endpoints.Jobs;

internal static partial class JobEvent
{
    private static class JobEventMapper
    {
        internal static JobEventRequest Map(JobEventApiModel apiModel)
        {
            return new JobEventRequest
            {
                Name = apiModel.Name!,
                Build = apiModel.Build!.Value,
                GitUrl = apiModel.GitUrl!,
                Commit = apiModel.Commit!,
                Status = apiModel.Status!.ToLowerInvariant() switch
                {
                    "started" => JobStatus.Started,
                    "success" => JobStatus.Succeeded,
                    "failure" => JobStatus.Failed,
                    _ => JobStatus.Aborted,
                },
                Url = apiModel.BuildUrl,
            };
        } 
    }
}