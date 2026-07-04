using connector_daemon.AzureIntegration.Models;

namespace connector_daemon.AzureIntegration.Exceptions;

internal sealed class AzureCommitNotFoundException(AzureRepo repo, string commit) :
    Exception($"Azure commit '{commit}' not found in repo '{repo.Name}' of project '{repo.Project.Name}'");