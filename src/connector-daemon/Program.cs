using connector_daemon.Endpoints.Jobs;
using connector_daemon.Persistence;
using connector_daemon.Services.EventRegistration;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IValidator<JobEvent.JobEventApiModel>, JobEvent.JobEventModelValidator>();
builder.Services.AddScoped<IJobEventRegistrar, JobEventRegistrar>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IJobEventRepository, JobEventRepository>();

var app = builder.Build();

app.MapJobEventWebhook();

var scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await dbContext.Database.MigrateAsync();
await scope.DisposeAsync();

app.Run();
