namespace dashboard.Integrations.Azure.Models;

internal sealed class AzurePullRequest
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string SourceBranch { get; set; }
}
