using dashboard.Endpoints.Health;
using dashboard.Endpoints.Jobs;
using dashboard.Integrations.Daemon;
using dashboard.Services;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IJobListService, JobListService>();
builder.Services.AddScoped<IDaemonClient, DaemonClient>();
builder.Services.Configure<DaemonClientOptions>(
    builder.Configuration.GetSection(nameof(DaemonClientOptions)));
builder.Services.AddHttpClient(nameof(DaemonClient), (sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<DaemonClientOptions>>().Value;
    client.BaseAddress = new Uri(options.UrlBase);
});
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
app.MapFallbackToFile("index.html");

app.Run();
