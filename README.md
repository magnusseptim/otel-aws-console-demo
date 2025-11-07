<p align="center">
  <img src="docs/banner.png" alt="Otel AWS Console Demo
" width="980">
</p>


# Otel AWS Console Demo

Small .NET app showing **traces + metrics + logs** with OpenTelemetry.

```ascii
+------------------------+
|  .NET demo app (API)   |
|  - System.Diagnostics  |
|    .Metrics Meter      |
|  - ObservableGauge     |
+-----------+------------+
            |
            | OTLP (gRPC 4317 / HTTP 4318)
            v
+------------------------+
|  OTel Collector        |
|  - OTLP receiver       |
|  - debug exporter      |  --> prints telemetry to console
+-----------+------------+
            ^
            |
         docker compose
  (app + collector sidecar)

```

Local run uses **OTLP → Collector (debug exporter)** so telemetry prints to console.

## Run

```bash
make up               # builds app image, starts Collector + app
make logs             # view Collector debug output
make hit              # hits /ping and /work to generate telemetry
make demo-compose     # dem- -> make up + make hit after 2 sec and tail -n 80 to print them on terminal. If you are lucky you may even spot tenant.id baggage added via processor
make down
```

## What it does

- **Traces**: GET /ping, GET /work (server spans) and a child GET https://www.example.com/ span.
- **Metrics**: demo.work.requests (Counter) and demo.queue.depth (ObservableGauge).
- **Logs ↔ Traces**: app logs show TraceId/SpanId correlation.

## How it’s wired

- App exports over OTLP (defaults: 4317 gRPC, 4318 HTTP) via OTEL_EXPORTER_OTLP_ENDPOINT.

- The Collector uses the debug exporter to print incoming telemetry (the logging exporter was removed; use debug).

- Config via env vars follows the OpenTelemetry spec (e.g., OTEL_PROPAGATORS, OTEL_RESOURCE_ATTRIBUTES, OTEL_TRACES_SAMPLER).
