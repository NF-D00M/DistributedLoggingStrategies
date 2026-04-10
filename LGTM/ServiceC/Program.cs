using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System.Diagnostics;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Service Identity
const string serviceName = "Lgtm.ServiceC";
const string serviceInstanceId = "localhost";

// Serilog sends logs directly to Loki (port: 3100)
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} [Trace: {TraceId}] [Span: {SpanId}]{NewLine}{Exception}")
    .WriteTo.GrafanaLoki("http://localhost:3100", new[] {
        new LokiLabel { Key = "service", Value = serviceName }
    })
    .CreateLogger();

builder.Host.UseSerilog();

// OpenTelemetry Tracing & Metrics
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName, serviceInstanceId: serviceInstanceId)) 
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation() 
        .AddOtlpExporter(opt => {
            opt.Endpoint = new Uri("http://localhost:4317");
            opt.Protocol = OtlpExportProtocol.Grpc;
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation() // Tracks request rates, errors, and durations
        .AddHttpClientInstrumentation() // Tracks downstream calls 
        .AddRuntimeInstrumentation()    // Tracks .NET GC, Memory, and ThreadPool
        .AddOtlpExporter(opt => {
            opt.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
            opt.Protocol = OtlpExportProtocol.HttpProtobuf;
        }));

WebApplication app = builder.Build();

app.MapGet("serviceC/data", (ILogger<Program> logger) => {
    logger.LogInformation("Service C processing request");

    return Results.Ok(new
    {
        TraceId = Activity.Current?.TraceId.ToString()
    });
});

app.MapGet("serviceC/error", (ILogger<Program> logger) =>
{
    logger.LogInformation("Service C processing request");
    throw new Exception("Forced error for testing");
});

app.Run("http://localhost:5003"); 