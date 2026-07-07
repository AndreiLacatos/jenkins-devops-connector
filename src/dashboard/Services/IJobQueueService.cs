namespace dashboard.Services;

internal interface IJobQueueService
{
    Task RequeueJobAsync(string name, int build, string commit, CancellationToken cancellationToken);
}