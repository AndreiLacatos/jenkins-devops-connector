namespace dashboard.Services.Models;

internal sealed class Job
{
    public string Name { get; set; }
    public int Build { get; set; }
    public string Commit { get; set; }
    public string JobEvent { get; set; }
    public string? BuildUrl { get; set; }
    public string SyncStatus { get; set; }
    public string RegisteredAt { get; set; }
    public string? EnqueuedAt { get; set; }
    public string? FinishedAt { get; set; }
}
