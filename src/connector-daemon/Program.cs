using connector_daemon.Endpoints.Jobs;
using connector_daemon.Services.EventRegistration;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<IValidator<JobEvent.JobEventApiModel>, JobEvent.JobEventModelValidator>();
builder.Services.AddScoped<IJobEventRegistrar, JobEventRegistrar>();
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.UseHttpsRedirection();

app.MapJobEventWebhook();

app.Run();
