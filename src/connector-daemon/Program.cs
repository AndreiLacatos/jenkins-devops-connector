using System.Threading.Channels;
using connector_daemon;
using connector_daemon.AzureIntegration;
using connector_daemon.Endpoints.Jobs;
using connector_daemon.HealthChecks;
using connector_daemon.Persistence;
using connector_daemon.Services.EventRegistration;
using connector_daemon.Services.JobEventProcessing;
using connector_daemon.Services.PullRequestConsistency;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using JobEventModel = connector_daemon.Services.EventRegistration.Models.JobEvent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IValidator<JobEvent.JobEventApiModel>, JobEvent.JobEventModelValidator>();
builder.Services.AddScoped<IJobEventRegistrar, JobEventRegistrar>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IJobEventRepository, JobEventRepository>();
builder.Services.AddScoped<IJobEventProcessingStatusRepository, JobEventRepository>();
builder.Services.AddScoped<IJobEventStatusNormalizer, JobEventStatusNormalizer>();
builder.Services.AddHostedService<JobEventWatcher>();
builder.Services.AddHostedService<JobEventDequeuer>();
builder.Services.AddHostedService<PullRequestWatcher>();
builder.Services.AddScoped<IJobEventEnqueuer, JobEventEnqueuer>();
builder.Services.AddScoped<IJobEventProcessor, JobEventProcessor>();
builder.Services.AddScoped<IPullRequestStatusReconciler, PullRequestStatusReconciler>();
builder.Services.AddSingleton(new MonitoredRepositories());
var channel = Channel.CreateUnbounded<JobEventModel>();
builder.Services.AddSingleton(_ => channel.Reader);
builder.Services.AddSingleton(_ => channel.Writer);
builder.Services.AddScoped<IAzureClient, AzureClient>();
builder.Services.Configure<AzureClientOptions>(
    builder.Configuration.GetSection(nameof(AzureClientOptions)));
builder.Services
    .AddHealthChecks()
    .AddCheck<AzureHealthCheck>(
        name: "AZURE",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy)
    .AddCheck<PersistenceHealthCheck>(
        name: "PERSISTENCE",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapJobEventWebhook();

var scope = app.Services.CreateAsyncScope();
var lifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
// migrate database
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await dbContext.Database.MigrateAsync(lifetime.ApplicationStopping);

// hydrate monitored repositories
var repository = scope.ServiceProvider.GetRequiredService<IJobEventRepository>();
var azureClient = scope.ServiceProvider.GetRequiredService<IAzureClient>();
var monitoredAzureRepos = scope.ServiceProvider.GetRequiredService<MonitoredRepositories>();
var monitoredGitRepos = await repository.ListMonitoredRepositoriesAsync(lifetime.ApplicationStopping);
foreach (var gitRepo in monitoredGitRepos)
{
    try
    {
        var azureRepo = await azureClient.GetRepositoryAsync(gitRepo, lifetime.ApplicationStopping);
        monitoredAzureRepos.Add(azureRepo);
    }
    catch
    {
        app.Logger.LogWarning("Failed to resolve Azure repository by git URL '{GitUrl}'", gitRepo);
    }
}

// requeue stale jobs
var normalizer = scope.ServiceProvider.GetRequiredService<IJobEventStatusNormalizer>();
await normalizer.ResetStaleJobEventsAsync(lifetime.ApplicationStopping);
await scope.DisposeAsync();

app.Run();
