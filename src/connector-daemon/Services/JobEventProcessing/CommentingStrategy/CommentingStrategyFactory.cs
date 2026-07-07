using Microsoft.Extensions.Options;

namespace connector_daemon.Services.JobEventProcessing.CommentingStrategy;

internal sealed class CommentingStrategyFactory : ICommentingStrategyFactory
{
    private readonly CommentingStrategyOptions _strategy;

    public CommentingStrategyFactory(IOptions<CommentingStrategyOptions> strategy)
    {
        _strategy = strategy.Value;
    }

    public ICommentingStrategy? GetCommentingStrategy()
    {
        return (_strategy.CommentingStrategy ?? string.Empty).ToLowerInvariant() switch
        {
            "none" => null,
            "single" => new SingleThreadStrategy(),
            _ => new JobThreadStrategy(),
        };
    }
}
