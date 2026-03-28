# MVFC.Aspire.Helpers.ApigeeEmulator

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.ApigeeEmulator)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.ApigeeEmulator)

Helpers for integrating the Google Apigee Emulator with .NET Aspire projects, enabling local API proxy development and testing.

## Motivation

Working with Apigee API proxies locally usually means:

- Spinning up the emulator container manually with the correct image and ports.
- Remembering to build and deploy the proxy bundle (ZIP) every time you make changes.
- Manually configuring TargetServers to point at your backend services.
- Dealing with `host.docker.internal` and port mismatches between your host and Docker.

With .NET Aspire you can define containers, but you still need to:

- Configure the emulator image, control port, and traffic port.
- Build and deploy your apiproxy bundle to the emulator on startup.
- Dynamically wire TargetServers to match the Aspire-allocated backend ports.

`MVFC.Aspire.Helpers.ApigeeEmulator` provides:

- `AddApigeeEmulator(...)` to start the emulator with sensible defaults.
- `.WithWorkspace(...)` to point at your local proxy bundle.
- `.WithEnvironment(...)` to set the Apigee environment name.
- `.WithBackend(...)` to automatically resolve Aspire backend endpoints as TargetServers.

## Overview

This project facilitates the configuration and integration of the Apigee Emulator in distributed .NET Aspire applications, providing extension methods to:

- Add the Apigee Emulator container with pre-configured ports.
- Deploy the proxy bundle (apiproxy) automatically on startup.
- Dynamically inject TargetServer configurations pointing at Aspire-managed backends.
- Merge static and dynamic TargetServer definitions for hybrid scenarios.

## Apigee Emulator advantages

- Develop and test API proxies locally without a Google Cloud account.
- Validate traffic policies, security flows, and SharedFlows before pushing to production.
- Full support for Trace/Debug sessions for request inspection.
- Seamless integration with existing Aspire-managed backend services.

## Compatible Images

- **Emulator**:
  - `gcr.io/apigee-release/hybrid/apigee-emulator` (Default in Aspire helper)

## Project Structure

- [`MVFC.Aspire.Helpers.ApigeeEmulator`](MVFC.Aspire.Helpers.ApigeeEmulator.csproj): Helpers and extensions library for Apigee Emulator.

## Features

- Adds the Apigee Emulator container with default image and ports.
- Deploys the proxy bundle automatically when the emulator is ready.
- Resolves Aspire backend ports and injects TargetServer configurations.
- Merges existing static `targetservers.json` with dynamically generated entries.
- Extension methods for fluent AppHost configuration.

## Installation

```sh
dotnet add package MVFC.Aspire.Helpers.ApigeeEmulator
```

## Quick Aspire usage (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.ApigeeEmulator;

var builder = DistributedApplication.CreateBuilder(args);

var apigeeWorkspace = Path.Combine(Directory.GetCurrentDirectory(), "apigee-workspace");

var api = builder.AddProject<Projects.MyApi>("my-api")
                 .WithHttpEndpoint(port: 5050);

var apigee = builder.AddApigeeEmulator("apigee-emulator")
                    .WithWorkspace(apigeeWorkspace)
                    .WithEnvironment("local")
                    .WithBackend(api);

await builder.Build().RunAsync();
```

## Ports

| Port | Default | Description |
|---|---|---|
| Control | `7070` → `8080` (container) | Management/deploy API |
| Traffic | `8998` → `8998` (container) | API gateway traffic |

## Provisioning diagram

```mermaid
sequenceDiagram
    participant Aspire as .NET Aspire
    participant Container as Apigee Emulator Container
    participant Hook as Deploy Lifecycle Hook
    participant Backend as Aspire Backend

    Aspire->>Container: Start container (apigee-emulator)
    Container-->>Aspire: Ready (control port available)
    Aspire->>Hook: Trigger AfterResourcesCreatedEvent
    Hook->>Backend: Wait for backend Running state
    Backend-->>Hook: Running (port resolved)
    Hook->>Hook: Build ZIP (merge targetservers.json)
    Hook->>Container: POST /v1/emulator/deploy
    Container-->>Hook: Deploy successful
    Hook-->>Aspire: Deployment completed
```

## Public methods

- `AddApigeeEmulator` – adds the emulator container with default image and ports.
- `WithWorkspace` – sets the local path to the apiproxy bundle.
- `WithEnvironment` – sets the Apigee environment name (default: `"local"`).
- `WithDockerImage` – overrides the Docker image and tag.
- `WithBackend` – configures an Aspire backend as a TargetServer for the proxy.

## Requirements

- .NET 9+
- Aspire.Hosting >= 9.5.0
- Docker running locally

## License

Apache-2.0
