using System.Threading.Channels;
using connector_daemon;
using connector_daemon.Endpoints.Jobs;
using connector_daemon.Persistence;
using connector_daemon.Services.EventRegistration;
using connector_daemon.Services.JobEventProcessing;
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
builder.Services.AddScoped<IJobEventEnqueuer, JobEventEnqueuer>();
builder.Services.AddScoped<IJobEventProcessor, JobEventProcessor>();
var channel = Channel.CreateUnbounded<JobEventModel>();
builder.Services.AddSingleton(_ => channel.Reader);
builder.Services.AddSingleton(_ => channel.Writer);

var app = builder.Build();

app.MapJobEventWebhook();

var scope = app.Services.CreateAsyncScope();
var lifetime = scope.ServiceProvider.GetRequiredService<IHostApplicationLifetime>();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await dbContext.Database.MigrateAsync(lifetime.ApplicationStopping);
var normalizer = scope.ServiceProvider.GetRequiredService<IJobEventStatusNormalizer>();
await normalizer.ResetStaleJobEventsAsync(lifetime.ApplicationStopping);
await scope.DisposeAsync();

app.Run();
