# DistributedLoggingStrategies

This application serves as a reference for a modern, vendor-neutral observability stack. By leveraging the **OpenTelemetry (OTel)** standard, the system decouples telemetry collection from storage, ensuring a flexible and scalable architecture.

---

## Introduction: The LGTM Stack & OpenTelemetry

The observability suite is built around the **LGTM** (Loki, Grafana, Tempo, Mimir/Prometheus) stack. Data is unified through **Grafana**, which acts as a centralised visualisation layer for three critical data types:

### 1. Centralised & Structured Logging (Loki)
Traditional logging often lacks context. This application utilises **Structured Logging** (typically in JSON format) to attach rich metadata to every event.
* **Queryable Intelligence:** Using **Loki**, logs are indexed by labels (e.g., `service_id`, `environment`). This allows for high-speed filtering using LogQL in Grafana, moving beyond simple text searches to complex data analysis.

### 2. Time-Series Metrics (Prometheus)
**Prometheus** tracks the health and performance of the application over time.
* **Performance Monitoring:** We capture "Golden Signals" (Latency, Traffic, Errors, and Saturation). These metrics are visualised in real-time dashboards to identify trends, spikes, or system degradation before they become critical failures.

### 3. Distributed Tracing (Tempo)
In a microservice environment, understanding the journey of a single request is vital. **Tempo** provides a high-scale backend for distributed tracing.
* **Trace Visualisation:** By assigning a unique `trace_id` to each request via OpenTelemetry, Tempo allows you to visualise the entire execution path. This makes it possible to pinpoint exactly which service or database query is causing a bottleneck.

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
* **Configuration:** Configure the application to emit OTLP traces. Ensure Tempo is listening for incoming gRPC or HTTP traffic (typically on port `4317` or `4318`).

### 4. Prometheus
* **Role:** Metrics collection and storage.
* **Configuration:** Define your scrape jobs in `prometheus.yml`. Ensure the application or the OTel Collector provides a `/metrics` endpoint for Prometheus to poll.

--- 

# Example C# Implementation

See file: `LgtmDistributedLogging/Program.cs` to view Open Telemetry exporting

---

## Run Service

### Grafana
Switch directory: `cd C:\Program Files\GrafanaLabs\grafana\bin`

Run service: `C:\Program Files\GrafanaLabs\grafana\bin`

### Loki
Switch directory: `cd C:\OtelStack\loki`

Run service: `.\loki-windows-amd64.exe --config.file=loki-config.yaml`

### Tempo
Switch directory: `cd C:\OtelStack\tempo`
  
Run service: `./tempo --config.file=tempo-config.yaml`

### Prometheus
Switch directory: `cd C:\OtelStack\prometheus\prometheus-3.10.0.windows-amd64`
  
Run service: `.\prometheus.exe --config.file=prometheus.yml --web.enable-otlp-receiver --web.enable-remote-write-receiver`

Once all services are running, access the local dashboard at:

`http://localhost:3000/`

---

# Dashboards

## Loki

View logs from distributed systems in one location

![Centralised Logging](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/loki-centralised-logging.png)

Query logs or filter by key

![Structured Logging](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/loki-structured-logging.png)

## Tempo

Trace end-to-end service requests by passing trace ids through HTTP headers.

![Distributed Tracing](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/tempo-distributed-tracing.png)

## Prometheus

Monitor and tailor your dashboard to include any of the 365 available metrics

![Metrics](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/prometheus-metrics.png)

## Prometheus & Tempo

Monitor spans

Tempo

![Spans Tempo](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/tempo-monitor-spans.png)

Prometheus

![Spans Prometheus](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/prometheus-monitor-spans.png)

Service Graph

![Monitor spans](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/service-graph.png)


## Import Exisiting Dashboards

Import existing dashboards from the Grafana community


![Imported Dashboard](https://github.com/NF-D00M/DistributedLoggingStrategies/blob/master/LGTM/LgtmDistributedLogging/Images/imported-dashboard.png)

## Creating Custom Dashboards

## Prometheus and Tempo Service Graph

