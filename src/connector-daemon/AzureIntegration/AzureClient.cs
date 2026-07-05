using connector_daemon.AzureIntegration.Exceptions;
using connector_daemon.AzureIntegration.Models;
using connector_daemon.Services.EventRegistration.Models;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace connector_daemon.AzureIntegration;

internal sealed class AzureClient : IAzureClient, IDisposable
{
    private const string StatusContextName = "Jenkins CI";
    private const string StatusContextGenre = "continuous-integration";

    public AzureClient(IOptions<AzureClientOptions> options)
    {
        var credentials = new VssBasicCredential(string.Empty, options.Value.Pat);
        _connection = new VssConnection(
            new Uri(options.Value.OrganizationUrl),
            credentials);
        _gitClient = _connection.GetClient<GitHttpClient>();
    }

    private readonly VssConnection _connection;
    private readonly GitHttpClient _gitClient;

    public async Task<AzureRepo> GetRepositoryAsync(string repoUrl, CancellationToken cancellationToken)
    {
        var repos = await _gitClient.GetRepositoriesAsync(cancellationToken: cancellationToken);
        // normalize the URLs by stripping leading 'https://dev.azure.com/', 'https://someone@dev.azure.com/'
        // or 'git@ssh://git@ssh.dev.azure.com:v3/' prefixed
        var normalizedTarget = NormalizeRepoUrl(repoUrl);
        var targetRepo = repos.FirstOrDefault(repo => NormalizeRepoUrl(repo.Url) == normalizedTarget
            || NormalizeRepoUrl(repo.WebUrl) == normalizedTarget
            || NormalizeRepoUrl(repo.SshUrl) == normalizedTarget
            || NormalizeRepoUrl(repo.RemoteUrl) == normalizedTarget);
        if (targetRepo is null)
        {
            throw new AzureRepoNotFoundException(repoUrl);
        }

        return new AzureRepo
        {
            Id = targetRepo.Id.ToString(),
            Name = targetRepo.Name,
            Project = new AzureProject
            {
                Id = targetRepo.ProjectReference.Id.ToString(),
                Name = targetRepo.ProjectReference.Name,
            },
        };
    }

    public async Task<AzureCommit> GetCommitAsync(AzureRepo repo, string commit, CancellationToken cancellationToken)
    {
        var targetCommit = await _gitClient.GetCommitAsync(
            repo.Project.Name,
            commit,
            repo.Id,
            cancellationToken: cancellationToken);

        if (targetCommit is null)
        {
            throw new AzureCommitNotFoundException(repo, commit);
        }

        var jenkinsStatus = (targetCommit.Statuses ?? [])
            .FirstOrDefault(s => s.Context.Name == StatusContextName && s.Context.Genre == StatusContextGenre);
        return new AzureCommit
        {
            Id = targetCommit.CommitId,
            LatestJenkinsStatus = jenkinsStatus is null ? null : new SynchronizedJenkinsStatus
            {
                State = MapAzureState(jenkinsStatus.State),
                CreationTime = jenkinsStatus.CreationDate,
            },
        };
    }

    public async Task<IEnumerable<AzurePullRequest>> ListActivePullRequestsAsync(AzureRepo repo, CancellationToken cancellationToken)
    {
        var pullRequests = await _gitClient.GetPullRequestsAsync(
            repo.Project.Name,
            repo.Id,
            new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Active,
            },
            cancellationToken: cancellationToken);
        foreach (var pr in pullRequests)
        {
            var commits = await _gitClient.GetPullRequestCommitsAsync(
                repo.Project.Name,
                pr.PullRequestId,
                cancellationToken: cancellationToken);
            pr.Commits = commits?.ToArray() ?? [];
        }
        var targetPrs = pullRequests.Where(pr => pr.Commits.Length != 0);
        var mappedPrs = new List<AzurePullRequest>();
        foreach (var pr in targetPrs)
        {
            var commit = pr.Commits.FirstOrDefault();
            // try to figure out the latest Jenkins status
            var iterations = await _gitClient.GetPullRequestIterationsAsync(
                pr.Repository.Id,
                pr.PullRequestId,
                cancellationToken: cancellationToken);
            var latestIteration = iterations.Max(i => i.Id);
            if (latestIteration is null)
            {
                // no luck
                mappedPrs.Add(new AzurePullRequest
                {
                    Id = pr.PullRequestId,
                    Title = pr.Title,
                    LatestJenkinsStatus = null,
                    LatestCommit = commit is null ? null : new AzureCommit
                    {
                        Id = commit.CommitId,
                        LatestJenkinsStatus = null,
                    },
                });
                continue;
            }

            var iterationCommits = await _gitClient.GetPullRequestIterationCommitsAsync(
                pr.Repository.Id,
                pr.PullRequestId,
                latestIteration.Value,
                cancellationToken: cancellationToken);
            var iterationCommit = iterationCommits.FirstOrDefault(c => c.CommitId == commit?.CommitId);
            if (iterationCommit is not null)
            {
                var commitStatuses = await _gitClient.GetStatusesAsync(
                    iterationCommit.CommitId,
                    pr.Repository.Id,
                    cancellationToken: cancellationToken);
                var latestCommitStatus = commitStatuses?.MaxBy(s => s.CreationDate);
                iterationCommit.Statuses = [latestCommitStatus];
            }
            var statuses = await _gitClient.GetPullRequestIterationStatusesAsync(
                    pr.Repository.Id,
                    pr.PullRequestId,
                    latestIteration.Value,
                    cancellationToken: cancellationToken);
            var latest = statuses?.MaxBy(s => s.CreationDate);
            if (latest is not null)
            {
                mappedPrs.Add(new AzurePullRequest
                {
                    Id = pr.PullRequestId,
                    Title = pr.Title,
                    LatestJenkinsStatus = new SynchronizedJenkinsStatus
                    {
                        State = MapAzureState(latest.State),
                        CreationTime = latest.CreationDate,
                    },
                    LatestCommit = iterationCommit is null ? null : new AzureCommit
                    {
                        Id = iterationCommit.CommitId,
                        LatestJenkinsStatus = (iterationCommit.Statuses?.Count ?? 0) == 0 ? null : new SynchronizedJenkinsStatus
                        {
                            State = MapAzureState(iterationCommit.Statuses![0].State),
                            CreationTime = iterationCommit.Statuses![0].CreationDate,
                        },
                    },
                });

            }
            else
            {
                // no luck
                mappedPrs.Add(new AzurePullRequest
                {
                    Id = pr.PullRequestId,
                    Title = pr.Title,
                    LatestJenkinsStatus = null,
                    LatestCommit = iterationCommit is null ? null : new AzureCommit
                    {
                        Id = iterationCommit.CommitId,
                        LatestJenkinsStatus = (iterationCommit.Statuses?.Count ?? 0) == 0 ? null : new SynchronizedJenkinsStatus
                        {
                            State = MapAzureState(iterationCommit.Statuses![0].State),
                            CreationTime = iterationCommit.Statuses![0].CreationDate,
                        },
                    },
                });
            }
        }

        return mappedPrs;
    }

    public async Task<IEnumerable<AzurePullRequest>> ListAssociatedActivePullRequestsAsync(
        AzureRepo repo, AzureCommit commit, CancellationToken cancellationToken)
    {
        var pullRequests = await _gitClient.GetPullRequestsByProjectAsync(
            repo.Project.Name,
            new GitPullRequestSearchCriteria
            {
                Status = PullRequestStatus.Active,
            },
            cancellationToken: cancellationToken);
        foreach (var pr in pullRequests)
        {
            var commits = await _gitClient.GetPullRequestCommitsAsync(
                repo.Project.Name,
                repo.Id,
                pr.PullRequestId,
                cancellationToken: cancellationToken);
            pr.Commits = commits?.ToArray() ?? [];
        }
        var targetPrs = pullRequests
            .Where(pr => pr.Commits.Any(c => c.CommitId == commit.Id));
        var mappedPrs = new List<AzurePullRequest>();
        foreach (var pr in targetPrs)
        {
           // try to figure out the latest Jenkins status
            var iterations = await _gitClient.GetPullRequestIterationsAsync(
                pr.Repository.Id,
                pr.PullRequestId,
                cancellationToken: cancellationToken);
            var latestIteration = iterations.Max(i => i.Id);
            if (latestIteration is null)
            {
                // no luck
                mappedPrs.Add(new AzurePullRequest
                {
                    Id = pr.PullRequestId,
                    Title = pr.Title,
                    LatestJenkinsStatus = null,
                    LatestCommit = null,
                });
                continue;
            }
            var statuses = await _gitClient.GetPullRequestIterationStatusesAsync(
                    pr.Repository.Id,
                    pr.PullRequestId,
                    latestIteration.Value,
                    cancellationToken: cancellationToken);
            var latest = statuses?.MaxBy(s => s.CreationDate);
            if (latest is not null)
            {
                mappedPrs.Add(new AzurePullRequest
                {
                    Id = pr.PullRequestId,
                    Title = pr.Title,
                    LatestJenkinsStatus = new SynchronizedJenkinsStatus
                    {
                        State = MapAzureState(latest.State),
                        CreationTime = latest.CreationDate,
                    },
                    LatestCommit = null,
                });

            }
            else
            {
                // no luck
                mappedPrs.Add(new AzurePullRequest
                {
                    Id = pr.PullRequestId,
                    Title = pr.Title,
                    LatestJenkinsStatus = null,
                    LatestCommit = null,
                });
            }
        }

        return mappedPrs;
    }

    public async Task SetCommitStatusAsync(
        AzureRepo repo, AzureCommit commit, JobEvent status, CancellationToken cancellationToken)
    {
        await _gitClient.CreateCommitStatusAsync(
            new GitStatus
            {
                State = MapAzureState(status.Status),
                Description = MapDescription(status.Status),
                TargetUrl = status.BuildUrl,
                Context = new GitStatusContext
                {
                    Name = StatusContextName,
                    Genre = StatusContextGenre,
                },
            },
            commit.Id,
            repo.Id,
            repo.Project.Name,
            cancellationToken);
    }

    public async Task SetPrStatusAsync(
        AzureRepo repo, AzurePullRequest pr, JobEvent status, CancellationToken cancellationToken)
    {
        var gitStatus = new GitPullRequestStatus
        {
            State = MapAzureState(status.Status),
            Description = MapDescription(status.Status),
            Context = new GitStatusContext
            {
                Name = StatusContextName,
                Genre = StatusContextGenre,
            },
            TargetUrl = status.BuildUrl,
        };
        var iterations = await _gitClient.GetPullRequestIterationsAsync(
            repo.Id,
            pr.Id,
            cancellationToken: cancellationToken);
        var latestIteration = iterations.Max(i => i.Id);

        await _gitClient.CreatePullRequestIterationStatusAsync(
            gitStatus,
            repo.Id,
            pr.Id,
            latestIteration!.Value,
            cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _gitClient.Dispose();
    }

    private static string NormalizeRepoUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return string.Empty;

        url = url.Trim();

        // remove credentials (https://user@host)
        var at = url.IndexOf('@');
        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) && at > 0)
            url = url[(at + 1)..];

        // strip https/http
        url = url.Replace("https://", "", StringComparison.OrdinalIgnoreCase)
            .Replace("http://", "", StringComparison.OrdinalIgnoreCase);

        // strip ssh prefix
        url = url.Replace("git@ssh.dev.azure.com:v3/", "", StringComparison.OrdinalIgnoreCase);

        // strip remaining host part for dev.azure.com style
        var idx = url.IndexOf("dev.azure.com/", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
            url = url[(idx + "dev.azure.com/".Length)..];

        return url.TrimEnd('/').ToLowerInvariant();
    }

    private static AzureStateEnum MapAzureState(GitStatusState s) => s switch
    {
        GitStatusState.NotSet => AzureStateEnum.NotSet,
        GitStatusState.Pending => AzureStateEnum.Pending,
        GitStatusState.Succeeded => AzureStateEnum.Succeeded,
        GitStatusState.Failed => AzureStateEnum.Failed,
        _ => AzureStateEnum.Other,
    };

    private static GitStatusState MapAzureState(JobStatus s) => s switch
    {
        JobStatus.Started => GitStatusState.Pending,
        JobStatus.Succeeded => GitStatusState.Succeeded,
        _ => GitStatusState.Failed,
    };

    private static string MapDescription(JobStatus s) => s switch
    {
        JobStatus.Started => "Jenkins build running",
        JobStatus.Succeeded => "Jenkins build succeeded",
        _ => "Jenkins build failed",
    };
}
