namespace connector_daemon.AzureIntegration.Exceptions;

internal sealed class AzureRepoNotFoundException(string repo) :
    Exception($"Azure repo '{repo}' not found");