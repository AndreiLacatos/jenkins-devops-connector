namespace connector_daemon.AzureIntegration.Models;

internal sealed class AzureThread
{
    public required int Id { get; init; }
    public required IEnumerable<AzureThreadComment> Comments { get; init; }

    public sealed class AzureThreadComment
    {
        public short Id { get; init; }
        public required string Content { get; init; }
        public DateTimeOffset PublishedAt { get; init; }
        public short? ParentId { get; set; }
    }
}
