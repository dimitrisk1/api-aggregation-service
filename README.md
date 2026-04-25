# API Aggregation Service

This solution implements a clean-architecture ASP.NET Core aggregation service that fetches data from multiple external APIs in parallel and exposes a single unified endpoint.

## Architecture

- `src/Domain`: core entities and provider abstractions
- `src/Application`: orchestration, filtering/sorting, metrics contracts, DTOs
- `src/Infrastructure`: external API integrations, caching, resilience, background monitoring, JWT token generation
- `src/Core-Api`: HTTP API, Swagger, dependency composition
- `ApiAggregator.Test`: unit tests

## External Providers

The service is wired to three public APIs that work without private credentials:

- Open-Meteo: current weather for a searched location
- GitHub REST API: repository search results
- Stack Exchange API: Stack Overflow question search results

Each provider implements `IExternalProvider`, making it straightforward to add new sources without changing the aggregation workflow.

## Endpoints

### `GET /api/aggregate`

Aggregates provider data in parallel and returns a unified result set.
Requires a valid JWT bearer token.

Query parameters:

- `query` (required): search term or location
- `category`: filter by category
- `source`: filter by provider name
- `fromUtc`: filter items on or after this UTC date
- `toUtc`: filter items on or before this UTC date
- `sortBy`: `date`, `relevance`, `title`, `source`
- `sortDirection`: `asc`, `desc`
- `limit`: maximum returned items, capped at 100

Example:

```http
GET /api/aggregate?query=dotnet&source=GitHub&sortBy=relevance&sortDirection=desc&limit=10
```

Response shape:

```json
{
  "query": "dotnet",
  "generatedAtUtc": "2026-04-25T18:30:00Z",
  "totalItems": 3,
  "items": [
    {
      "source": "GitHub",
      "title": "dotnet/runtime",
      "description": "Core runtime libraries",
      "category": "C#",
      "url": "https://github.com/dotnet/runtime",
      "relevanceScore": 12345,
      "date": "2026-04-25T17:50:00Z"
    }
  ],
  "providers": [
    {
      "providerName": "GitHub",
      "status": "Success",
      "itemCount": 10,
      "responseTimeMs": 83.74,
      "errorMessage": null
    }
  ],
  "totalProcessingTimeMs": 115.49
}
```

Provider status values:

- `Success`: live upstream response used
- `Fallback`: upstream failed and cached data was returned
- `Failed`: upstream failed and no fallback data was available

### `GET /api/stats`

Returns per-provider in-memory request statistics:
Requires a valid JWT bearer token.

- total requests
- successful requests
- failed requests
- lifetime average response time
- last 5 minute average response time
- performance buckets: `fast`, `average`, `slow`

### `POST /auth/token`

Public helper endpoint for JWT generation.

Request body:

```json
{
  "username": "demo-user",
  "role": "User"
}
```

Use the returned token as `Authorization: Bearer <token>` when calling protected endpoints.

## Resilience and Performance

- Parallel provider execution via `Task.WhenAll`
- In-memory caching to reduce redundant upstream calls
- Cached fallback when an upstream API fails
- Polly retry, timeout, and circuit breaker policies
- Thread-safe in-memory statistics store
- Background monitor that logs performance anomalies when the recent average exceeds the lifetime average by 50%

## Configuration

`src/Core-Api/appsettings.json`

- `Jwt:Enabled`: enables JWT validation when `true`
- `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`: JWT settings
- `BackgroundService:IsActive`: enables anomaly monitoring
- `BackgroundService:Timer`: monitor interval in seconds

JWT is enabled in the current solution configuration. `GET /api/aggregate` and `GET /api/stats` both require a bearer token.

## Run

```bash
dotnet restore Core.slnx
dotnet run --project src/Core-Api/Core-Api.csproj
```

Swagger is available at the application root in development.

## Test

```bash
dotnet test Core.slnx
```
