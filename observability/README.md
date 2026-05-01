# Observability Stack

This folder contains a local OpenTelemetry + Prometheus + Grafana setup for the API aggregation service.

## Services

- OpenTelemetry Collector
- Prometheus
- Grafana

## Ports

- Collector OTLP HTTP: `4318`
- Collector OTLP gRPC: `4317`
- Collector Prometheus export: `9464`
- Prometheus UI: `9090`
- Grafana UI: `3000`

## Start the stack

From the repository root:

```powershell
docker compose -f observability/docker-compose.yml up -d
```

## Enable OTLP export in the API

In [appsettings.json](C:/Users/jkon2/source/repos/Core/src/Core-Api/appsettings.json) or your local development config:

```json
"OpenTelemetry": {
  "ServiceName": "core-api-aggregator",
  "ConsoleExporterEnabled": true,
  "OtlpExporterEnabled": true,
  "OtlpEndpoint": "http://localhost:4318"
}
```

## Run the API

```powershell
dotnet run --project src/Core-Api/Core-Api.csproj
```

## Generate telemetry

1. Request a token from `POST /auth/token`
2. Call `GET /api/aggregate?query=dotnet`
3. Call `GET /api/aggregate?query=madrid`
4. Call `GET /api/stats`

These requests will generate:

- ASP.NET Core request telemetry
- `HttpClient` provider-call telemetry
- custom aggregation telemetry:
  - `aggregation.requests`
  - `aggregation.duration.ms`
  - `aggregation.provider.executions`

## View the dashboards

### Prometheus

Open:

[http://localhost:9090](http://localhost:9090)

Try queries such as:

- `aggregation_requests_total`
- `aggregation_provider_executions_total`
- `http_server_request_duration_seconds`

### Grafana

Open:

[http://localhost:3000](http://localhost:3000)

Default login:

- username: `admin`
- password: `admin`

Prometheus is provisioned automatically as the default datasource.

In Grafana Explore, try:

- `aggregation_requests_total`
- `aggregation_provider_executions_total`
- `aggregation_duration_ms`

## Stop the stack

```powershell
docker compose -f observability/docker-compose.yml down
```
