using dashboard.Endpoints.Health;
using dashboard.Endpoints.Jobs;
using dashboard.Integrations.Azure;
using dashboard.Integrations.Daemon;
using dashboard.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IJobListService, JobListService>();
builder.Services.AddScoped<IJobQueueService, JobQueueService>();
builder.Services.AddScoped<IDaemonClient, DaemonClient>();
builder.Services.Configure<DaemonClientOptions>(
    builder.Configuration.GetSection(nameof(DaemonClientOptions)));
builder.Services.AddHttpClient(nameof(DaemonClient), (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<DaemonClientOptions>>().Value;
    client.BaseAddress = new Uri(options.UrlBase);
});
builder.Services.AddScoped<AzureClient>();
builder.Services.AddScoped<IAzureClient>(
    sp => new CachingAzureClient(sp.GetRequiredService<AzureClient>(), sp.GetRequiredService<TimeProvider>()));
builder.Services.Configure<AzureClientOptions>(
    builder.Configuration.GetSection(nameof(AzureClientOptions)));
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.MapStaticAssets();
app.MapDaemonHealth();
app.MapJobList();
app.MapRequeueJob();
app.MapFallbackToFile("index.html");

app.Run();
