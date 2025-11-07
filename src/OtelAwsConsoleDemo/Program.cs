using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Context.Propagation;
using OtelAwsConsoleDemo.Extensions;
using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Extensions.AWS.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "otel-aws-console-demo";
const string serviceVersion = "1.0.0";

// Otlp

var meter = new Meter(serviceName, serviceVersion);
var workRequests = meter.CreateCounter<long>(
    "demo.work.requests",
    unit: "{request}",
    description: "Work endpoint request"
);

var queueDepth = 0;
var queueGauge = meter.CreateObservableGauge<long>(
    "demo.queue.depth",
    () => new Measurement<long>(queueDepth));

// Resources
builder.Services.Configure<LoggerFilterOptions>(o => o.MinLevel = LogLevel.Information);

var hasOtlp =
    !string.IsNullOrWhiteSpace(
        builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ??
        builder.Configuration["OTEL_EXPORTER_OTLP_TRACES_ENDPOINT"]);

// Logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName: serviceName));
    options.AddConsoleExporter();
    if (hasOtlp) options.AddOtlpExporter();
});

// Metrics and Traces
builder.Services.AddHttpClient();
builder.Services.AddOpenTelemetry()
.ConfigureResource(builder => builder.AddService(serviceName: serviceName))
.WithTracing(tracingBuilder =>
    tracingBuilder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddProcessor(new BaggageSpanProcessor())
        .AddConsoleExporter()
        .AddIf(hasOtlp, x => x.AddOtlpExporter())
        .AddXRayTraceId()
)
.WithMetrics(metricsBuilder =>
{
    metricsBuilder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddIf(hasOtlp, x => x.AddOtlpExporter());
});

Sdk.SetDefaultTextMapPropagator(new AWSXRayPropagator());

var app = builder.Build();

// Endpoints

app.MapGet("/ping", () => Results.Ok(new { pong = true }));

app.MapGet("/work", async ([FromServices] IHttpClientFactory http, [FromServices] ILogger<Program> log) =>
{
    Baggage.Current = Baggage.Current.SetBaggage("tenant.id", "acme-co"); // demo value
    workRequests.Add(1, KeyValuePair.Create<string, object?>("result", "ok"));
    queueDepth = Random.Shared.Next(0, 5);

    log.LogInformation("Starting /work");
    var client = http.CreateClient();
    var resp = await client.GetAsync("https://www.example.com/");
    log.LogInformation("Finished /work with {StatusCode}", (int)resp.StatusCode);
    return Results.Ok(new { status = (int)resp.StatusCode });
});

app.Run();