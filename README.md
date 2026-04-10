# DistributedLoggingStrategies

This application serves as a reference for a modern and agnostic observability stack. By leveraging the **OpenTelemetry (OTel)** standard, the system decouples telemetry collection from storage, ensuring a flexible and scalable architecture.

---

## Introduction: The LGTM Stack & OpenTelemetry

The observability suite is built around the **LGTM** (Loki, Grafana, Tempo, Mimir/Prometheus) stack. Data is unified through **Grafana**, which acts as a centralised visualisation layer for three critical data services:

### 1. Centralised & Structured Logging (Loki)
This application utilises **Structured Logging** (JSON format) to attach rich metadata to every event.
* **Queryable Intelligence:** Using **Loki**, logs are indexed by labels (`service_id`, `environment`). This allows for rapid filtering using LogQL in Grafana.

### 2. Time-Series Metrics (Prometheus)
**Prometheus** tracks the health and performance of the application over time.
* **Performance Monitoring:** Captures "Golden Signals" (Latency, Traffic, Errors, and Saturation). These metrics are visualised in real-time dashboards to identify trends, spikes, or system degradation before they become critical failures.

### 3. Distributed Tracing (Tempo)
**Tempo** provides a high-scale backend for distributed tracing.
* **Trace Visualisation:** By assigning a unique `trace_id` to each request via OpenTelemetry, Tempo allows you to visualise the entire request path. This makes it possible to pinpoint exactly which service or database query is causing a bottleneck.

---

## Prerequisites

Before running the application, you must ensure the following components are installed and configured. 

### 1. Grafana
* **Role:** The visualisation and dashboarding engine.
* **Configuration:** Ensure it is accessible (default port `3000`). You will need to configure Loki, Prometheus, and Tempo as active data sources.

### 2. Loki
* **Role:** Log aggregation system.
* **Configuration:** Set up the OpenTelemetry Collector to export logs to the Loki API. Ensure your labels are consistent to allow for efficient cross-referencing with traces.

### 3. Tempo
* **Role:** Distributed tracing storage.
* **Configuration:** Configure the application to emit OTLP traces. Ensure Tempo is listening for incoming gRPC or HTTP traffic (ports `4317` or `4318`).

### 4. Prometheus
* **Role:** Metrics collection and storage.
* **Configuration:** Define your scrape jobs in `prometheus.yml`. Ensure the application or the OTel Collector provides a `/metrics` endpoint for Prometheus to poll.

--- 

## Example C# Implementation

See file: `LgtmDistributedLogging/Program.cs` to view Open Telemetry exporting.

## Example Configuraion of Data Services 

See dir: `ServiceConfig` to view Loki, Tempo and Prometheus configruation.

---

## Run Service

### Grafana
Switch directory: `cd <path-to-grafana>\GrafanaLabs\grafana\bin`

Run service: `<path-to-grafana>\GrafanaLabs\grafana\bin`

### Loki
Switch directory: `cd <path-to-loki>\loki`

Run service: `.\loki-windows-amd64.exe --config.file=loki-config.yaml`

### Tempo
Switch directory: `cd <path-to-tempo>\tempo`
  
Run service: `./tempo --config.file=tempo-config.yaml`

### Prometheus
Switch directory: `cd <path-to-prometheus>\prometheus\prometheus-3.10.0.windows-amd64`
  
Run service: `.\prometheus.exe --config.file=prometheus.yml --web.enable-otlp-receiver --web.enable-remote-write-receiver`

Once all services are running, access Grafana:

`http://localhost:3000/`

---

# Dashboards

## Loki

View logs from distributed systems in one location

* **Correlation via Shared Labels:** Loki uses the same metadata labels as Prometheus to navigate instantly from a metric spike (high latency in Lgtm.ServiceC) to the specific logs for that service without re-filtering.

* **Unified Trace ID Stitching:** By searching for a single TraceId chronological timelines of logs from all services become available, making it easy to track how a request traveled (and where it failed) across distributed architecture.

![Centralised Logging](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/loki-centralised-logging.png)

Query logs or filter by key

* **Log-to-Metric Power:** Using LogQL queries transform raw text into real-time metrics on the fly, such as counting specific error strings or calculating request rates from logs. 

![Structured Logging](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/loki-structured-logging.png)

## Tempo

Trace end-to-end service requests by passing trace ids through HTTP headers.

* **Visualising Request Flow:** Tempo sees the exact path a single request takes as it hops through a service, revealing exactly how long the service spent waiting on another service, versus its own internal processing.

* **Pinpointing Latency Bottlenecks:** Break down a trace into individual span, to identify the specific method, database query, or downstream API call causing a delay, rather than just knowing the entire service is slow.

* **Root Cause Correlation:** Tempo acts as the connective tissue of the LGTM stack by clicking a span to see the exact Loki logs or Prometheus metrics associated with that specific operation, which eliminates manual searching during an incident.

![Distributed Tracing](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/tempo-distributed-tracing.png)

## Prometheus

Monitor and tailor your dashboard to include any of the 365 available metrics

* **Dimensional Data Model:** Prometheus uses key-value pairs called labels to categorise metrics to disect services and compare performance or group error rates by specific endpoints.

* **Pull-Based Scalability:** Prometheus scrapes metrics at regular intervals, which prevents your services from being overwhelmed by monitoring traffic during high-load events or retry storms.

* **Proactive Alerting:** Define complex mathematical thresholds ("Alert if P95 latency is > 2s for 5 minutes) that trigger notifications before a minor saturation issue turns into a total system outage.

![Metrics](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/prometheus-metrics.png)

## Prometheus & Tempo

Monitor spans with a summaries and relationships between Prometheus and Tempo data.

### Tempo

* **Tempo (Individual Detail):** Tempo stores the full raw trace data for spans, enabling specific request analsyis to see the exact sequence of events, parent-child relationships, and timing of every internal hop across a distributed system.

![Spans Tempo](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/tempo-monitor-spans.png)

### Prometheus

* **Prometheus (Aggregated Metrics):** Prometheus tracks spans by aggregating them into high-level metrics (via Span Metrics), allowing visibility over RED signals (Rate, Errors, and Duration) for every service and operation at scale without the need to examine individual traces.

![Spans Prometheus](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/prometheus-monitor-spans.png)

### Service Graph

* **Service Graph:** This feature dynamically maps the topography of your system by analysing trace data to visualise how services interact, automatically calculating request rates and latencies between them. View these performance metrics at a glacne to identify where traffic is bottlenecking or failing in the communication chain.

![Monitor spans](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/service-graph.png)


## Import Exisiting Dashboards

Import existing dashboards from the Grafana community

* **Instant Expert Visualisation:** Importing dashboards from the Grafana community leverages pre-built, battle-tested templates using unique Dashboard IDs. This instantly provides professional-grade visualisations for common tools in the LGTM stack or .NET runtimes without having to build every panel from scratch.

.NET ASP (ID:19924)

![Imported Dashboard](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/imported-dashboard.png)

## Creating Custom Dashboards

### Golden signals

The Four Golden Signals are the essential metrics for monitoring any user-facing distributed system. They provide a high-level view of system health and are the first things to check during an incident.

* **Latency:** The time it takes to service a request measured in milliseconds. It is critical to track the latency of successful requests separately from failed requests to ensure error-related fast-failures don't mask slow performance.

* **Traffic:** A measure of how much demand is being placed on your system, typically tracked by the number of HTTP requests per second or concurrent active sessions across services.

* **Errors:** The rate of requests that fail, either explicitly (HTTP 500s), implicitly ("success" response with the wrong data), or by policy (a request that takes over 10 seconds and is terminated).

* **Saturation:** A measure of how full a service is, highlighting the most constrained resources (CPU, memory, or thread pools) and indicating at what point performance will begin to degrade as the system reaches its maximum capacity.

![Golden Signals](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/golden-signals.png)


