namespace connector_daemon.Persistence;

internal class JobEventListFilter
{
    public int? Build { get; init; }
    public string? JobName { get; init; }
    public string? Commit { get; init; }
}
