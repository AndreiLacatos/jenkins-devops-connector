using dashboard.Services.Models;

namespace dashboard.Services;

internal interface IJobListService
{
    Task<IEnumerable<RepositoryJobs>> ListMostRecentJobsByRepositoriesAsync(CancellationToken cancellationToken);
}