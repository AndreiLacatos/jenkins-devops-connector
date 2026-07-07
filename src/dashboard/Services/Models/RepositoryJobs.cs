namespace dashboard.Services.Models;

internal sealed class RepositoryJobs
{
    public required string RepositoryName { get; init; }
    public required Dictionary<string, Dictionary<string, Job>> BranchJobs { get; init; }
}
