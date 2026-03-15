# MVFC.Aspire.Helpers.Gotenberg

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.Gotenberg)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Gotenberg)


Helpers for integrating with Gotenberg (API for converting documents and PDFs) in .NET Aspire projects.

## Motivation

Running Gotenberg locally usually means:

- Pulling the correct Docker image/tag.
- Mapping ports and exposing the HTTP endpoint to your applications.
- Hard‑coding the base URL of the PDF service in code or configuration.

With .NET Aspire you can model the container, but you still need to:

- Configure image, ports and health checks.
- Decide how to expose the base URL to each project.
- Keep host/port details in sync between the app host and your services.

`MVFC.Aspire.Helpers.Gotenberg` focuses exactly on this:

- `AddGotenberg(...)` starts the official `gotenberg/gotenberg:8` image with an HTTP endpoint and health check.
- `project.WithReference(gotenberg)` injects `GOTENBERG__BASE_URL` so your API/worker can just read it from configuration.

## Overview

This project allows you to easily add and integrate Gotenberg as a managed resource in distributed .NET Aspire applications. It simplifies provisioning the Gotenberg container and provides extension methods for configuring it in the AppHost.

## Project Structure

- [`MVFC.Aspire.Helpers.Gotenberg`](MVFC.Aspire.Helpers.Gotenberg.csproj): Helpers and extensions library for Gotenberg.

## Features

- Adds the Gotenberg container to the Aspire application.
- Manages automatic health check at `/health`.
- Automatically injects the service's base URL into the consuming project.
- Extension methods for quick configuration of custom ports.

## Compatible Images

- `gotenberg/gotenberg` (default tag `8`)

## Installation

Add the NuGet package to your AppHost project:

```sh
dotnet add package MVFC.Aspire.Helpers.Gotenberg
```

## Quick Aspire usage (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.Gotenberg;

var builder = DistributedApplication.CreateBuilder(args);

// Adds the Gotenberg container on host port 3000
var gotenberg = builder.AddGotenberg("gotenberg", port: 3000);

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-example")
       .WithReference(gotenberg)
       .WaitFor(gotenberg);

await builder.Build().RunAsync();
```

## Reference in backend project (API, Web, etc.)

When using `.WithReference(gotenberg)`, the AppHost will automatically inject an environment variable containing the accessible Gotenberg address so the consuming application can connect:

- `GOTENBERG__BASE_URL` = `http://localhost:<port>`

Use that value to configure your HTTP client pointing to Gotenberg.

## Fluent methods

| Method                      | Description                                 |
|----------------------------|---------------------------------------------|
| `WithDockerImage(image, tag)` | Overrides the Docker image or its tag used. |

## `AddGotenberg` parameters

| Parameter | Type   | Default | Description                                  |
|----------|--------|---------|----------------------------------------------|
| `name`   | string | —       | Resource name in Aspire.                     |
| `port`   | int    | `3000`  | HTTP port exposed on the host for communication. |

## Port details and visualization

- Default mapped port: `3000` (internally also `3000`).
- Health check: `http://localhost:<port>/health`.

## Requirements

- .NET 9+
- Aspire.Hosting >= 9.5.0

## License

Apache-2.0
