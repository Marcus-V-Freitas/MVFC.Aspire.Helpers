# MVFC.Aspire.Helpers.GcpFirestore

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010%20%7C%2011-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.GcpFirestore)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.GcpFirestore)

Helpers for integrating with Google Cloud Firestore in .NET Aspire projects, including support for the emulator.

## Motivation

Firestore project IDs are evaluated when the consuming project environment is built, so configurations may be added before or after `WithReference`.

Working with Google Cloud Firestore locally usually means:

- Spinning up an emulator container by hand.
- Remembering ports, project IDs and environment variables.
- Manually configuring readiness checks for the emulator.

With .NET Aspire you can define containers, but you still need to:

- Configure the emulator image and its ports.
- Keep emulator environment variables in sync across projects.
- Define project configurations in a consistent way before the application runs.

`MVFC.Aspire.Helpers.GcpFirestore` provides:

- `AddGcpFirestore(...)` to start the emulator.
- `WithFirestoreConfigs(...)` to describe project configurations in code.
- `WithReference(...)` to wire projects to the emulator and inject connection configurations automatically.

## Overview

This project facilitates the configuration and integration of Google Cloud Firestore in distributed .NET Aspire applications, providing extension methods to:

- Add the Google Cloud Firestore emulator.
- Configure project IDs automatically upon startup.
- Properly inject the emulator host connection string for automatic detection by Firestore clients.

## Firestore emulator advantages

- Simulates Firestore databases locally for development and testing.
- Allows testing data operations without depending on Google Cloud infrastructure.
- Facilitates development of robust data storage implementations.

## Compatible Images

- **Emulator**:
  - `mtlynch/firestore-emulator` (Default in Aspire helper)

## Project Structure

- [`MVFC.Aspire.Helpers.GcpFirestore`](MVFC.Aspire.Helpers.GcpFirestore.csproj): Helpers and extensions library for Firestore.

## Features

- Adds the Google Cloud Firestore emulator.
- Configures project IDs according to configuration.
- An HTTP health check probes the emulator root endpoint. Use `WaitFor(firestore)` to wait for this check; it does not validate authentication or data operations.
- Extension methods to facilitate AppHost configuration.

## Installation

```sh
dotnet add package MVFC.Aspire.Helpers.GcpFirestore
dotnet add package Aspire.Hosting.Dotnet --version 17.0.0
```

Commands and examples using `AddDotnetProject` target a .NET 10/11 AppHost on Aspire 17. For .NET 9 AppHosts, keep Aspire 13.6.5, omit `Aspire.Hosting.Dotnet` 17 and use `AddProject<Projects.MyApi>("api")`. Adjust the project path to your AppHost.

## Quick Aspire usage (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.GcpFirestore;
using MVFC.Aspire.Helpers.GcpFirestore.Models;

var builder = DistributedApplication.CreateBuilder(args);

var firestoreConfig = new FirestoreConfig(
    projectId: "test-project");

var firestore = builder.AddGcpFirestore("gcp-firestore")
                       .WithFirestoreConfigs(firestoreConfig);

builder.AddDotnetProject("api-example", "../MVFC.Aspire.Helpers.Playground.Api/MVFC.Aspire.Helpers.Playground.Api.csproj")
       .WithReference(firestore)
       .WaitFor(firestore);

await builder.Build().RunAsync();
```

## Emulated Resources Configuration

### `FirestoreConfig`

| Parameter   | Type   | Default | Description                       |
| ----------- | ------ | ------- | --------------------------------- |
| `projectId` | string | —       | GCP project ID used by Firestore. |

## Ports

- **HTTP Port:** `8084` *(mapped to internal port `8080` in the container)*

## Provisioning diagram

```mermaid
sequenceDiagram
    participant Aspire as .NET Aspire
    participant Container as Firestore Emulator Container
    participant App as Consumer

    Aspire->>Container: Start container (mtlynch/firestore-emulator)
    Aspire->>Container: Probe GET / on allocated HTTP endpoint
    Container-->>Aspire: HTTP health check passes
    Aspire->>App: Start consumer with emulator host and configured project IDs (WaitFor)
```

## Public methods

- `AddGcpFirestore` – adds the emulator container.
- `WithFirestoreConfigs` – configures project IDs.
- `WithDockerImage` – replaces the Docker image used by the resource.
- `WithReference` – wires projects to the emulator and sets the `FIRESTORE_EMULATOR_HOST` environment variable automatically.

## Default image

The default image is pinned to a tested SHA256 digest. Select another image/tag with `WithDockerImage(image, tag)` or Aspire image methods; the override clears the previous digest. See [runtime contracts](../../docs/runtime-contracts.md) for image maintenance, readiness and emulator limitations.

## Requirements

- .NET 9, 10 or 11
- Aspire.Hosting 13.6.5 on .NET 9; Aspire.Hosting 17.0.0 on .NET 10/11. AppHost examples use `Aspire.Hosting.Dotnet` 17 and `AddDotnetProject`; .NET 9 AppHosts retain the `ProjectResource` model.
- Docker running locally
- Google.Cloud.Firestore >= 4.7.0

## License

Apache-2.0
