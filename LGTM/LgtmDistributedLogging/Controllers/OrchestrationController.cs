using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LgtmDistributedLogging.Controllers;

[ApiController]
[Route("[controller]")]
public class OrchestrationController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<OrchestrationController> _logger;
    private static readonly ActivitySource ActivitySource = new("Lgtm.Orchestrator");

    public OrchestrationController(IHttpClientFactory clientFactory, ILogger<OrchestrationController> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    [HttpGet("publish")]
    public async Task<IActionResult> RunDistributedFlow()
    {
        // Start a Span
        using Activity? activity = ActivitySource.StartActivity("ComplexOrchestration");

        // Add a Baggage/Tag to follow request
        activity?.SetTag("operation.type", "ObservabilityTest");

        _logger.LogInformation("Starting distributed trace test across 3 services.");

        try
        {
            // Get ids from Activity 
            var traceId = Activity.Current?.TraceId.ToString();
            var spanId = Activity.Current?.SpanId.ToString();

            // Call Service A
            var clientA = _clientFactory.CreateClient("ServiceA");
            _logger.LogInformation($"Calling Service A... [TraceID: {traceId}, SpanID: {spanId}]");
            _ =  clientA.GetAsync("serviceA/data");

            // Call Service B
            var clientB = _clientFactory.CreateClient("ServiceB");
            _logger.LogInformation($"Calling Service B... [TraceID: {traceId}, SpanID: {spanId}]");
            _ = clientB.GetAsync("serviceB/data");

            // Call Service C
            var clientC = _clientFactory.CreateClient("ServiceC");
            _logger.LogInformation($"Calling Service C... [TraceID: {traceId}, SpanID: {spanId}]");
            _ = clientC.GetAsync("serviceC/data");
            _ = clientC.GetAsync("serviceC/error");

            return Ok(new { TraceId = Activity.Current?.TraceId.ToString(), Status = "Success" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Flow failed during service orchestration");
            return StatusCode(500, "Flow interrupted");
        }
    }
}