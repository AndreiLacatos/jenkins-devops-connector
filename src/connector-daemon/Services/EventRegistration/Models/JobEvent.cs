namespace connector_daemon.Services.EventRegistration.Models;

internal sealed class JobEvent
{
    public required string Name { get; init; }
    public required int Build { get; init; }
    public required string GitUrl { get; init; }
    public required string Commit { get; init; }
    public required JobStatus Status { get; init; }
    public required string? BuildUrl { get; init; }
    public required DateTimeOffset RegisteredAt { get; set; }

    internal static JobEvent FromRequest(JobEventRequest request, DateTimeOffset timeOffset) => new JobEvent
    {
        Build = request.Build,
        Commit = request.Commit,
        Name = request.Name,
        GitUrl = request.GitUrl,
        Status = request.Status,
        BuildUrl = request.Url,
        RegisteredAt = timeOffset,
    };
}
