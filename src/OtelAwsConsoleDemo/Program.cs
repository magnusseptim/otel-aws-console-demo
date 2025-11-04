using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Resources
const string serviceName = "otel-aws-console-demo";
builder.Services.Configure<LoggerFilterOptions>(o => o.MinLevel = LogLevel.Information);


// Logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName));
    options.AddConsoleExporter();
});

// Metrics and Traces
builder.Services.AddHttpClient();
builder.Services.AddOpenTelemetry()
.ConfigureResource(builder => builder.AddService(serviceName: serviceName))
.WithTracing(tracingBuilder =>
    tracingBuilder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
)
.WithMetrics(metricsBuilder =>
{
    metricsBuilder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter();
});

var app = builder.Build();

// Endpoints

app.MapGet("/ping", () => Results.Ok(new { pong = true }));

app.MapGet("/work", async ([FromServices] IHttpClientFactory http, [FromServices] ILoggerFactory lf) =>
{
    var log = lf.CreateLogger("demo");
    log.LogInformation("Starting /work");
    var client = http.CreateClient();
    // small outbound call to create an HttpClient child span
    var resp = await client.GetAsync("https://www.example.com/");
    log.LogInformation("Finished /work with {StatusCode}", (int)resp.StatusCode);
    return Results.Ok(new { status = (int)resp.StatusCode });
});

app.Run();