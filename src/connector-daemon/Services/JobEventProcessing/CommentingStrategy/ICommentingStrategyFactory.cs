namespace connector_daemon.Services.JobEventProcessing.CommentingStrategy;

internal interface ICommentingStrategyFactory
{
    ICommentingStrategy? GetCommentingStrategy();
}
