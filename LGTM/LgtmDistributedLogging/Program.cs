using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Service Identity
const string serviceName = "Lgtm.Orchestrator";
const string serviceInstanceId = "localhost";

// Serilog sends logs directly to Loki (port: 3100)
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.GrafanaLoki("http://localhost:3100", new[] {
        new LokiLabel { Key = "service", Value = serviceName }
    })
    .CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Services
builder.Services.AddHttpClient("ServiceA", c => c.BaseAddress = new Uri("http://localhost:5001/"));
builder.Services.AddHttpClient("ServiceB", c => c.BaseAddress = new Uri("http://localhost:5002/"));
builder.Services.AddHttpClient("ServiceC", c => c.BaseAddress = new Uri("http://localhost:5003/"));

// OpenTelemetry Tracing & Metrics
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName, serviceInstanceId: serviceInstanceId))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        //.AddConsoleExporter() 
        .AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri("http://localhost:4317");
            opt.Protocol = OtlpExportProtocol.Grpc;
        }))
    .WithMetrics(static metrics => metrics
        .AddAspNetCoreInstrumentation() // Tracks request rates, errors, and durations
        .AddHttpClientInstrumentation() // Tracks downstream calls ( Orchestrator -> Service A)
        .AddRuntimeInstrumentation()    // Tracks .NET GC, Memory, and ThreadPool
        //.AddConsoleExporter()
        .AddOtlpExporter(opt => {
            opt.Endpoint = new Uri("http://localhost:9090/api/v1/otlp/v1/metrics");
            opt.Protocol = OtlpExportProtocol.HttpProtobuf;
        }));

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();


// Start application
try
{
    Log.Information("Starting Lgtm.Orchestrator up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}