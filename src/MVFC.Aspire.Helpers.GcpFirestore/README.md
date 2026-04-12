# MVFC.Aspire.Helpers.GcpFirestore

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.GcpFirestore)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.GcpFirestore)

Helpers for integrating with Google Cloud Firestore in .NET Aspire projects, including support for the emulator.

## Motivation

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
- Native TCP port health checks ensure the emulator is fully ready before projects start consuming it.
- Extension methods to facilitate AppHost configuration.

## Installation

```sh
dotnet add package MVFC.Aspire.Helpers.GcpFirestore
```

## Quick Aspire usage (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.GcpFirestore;
using MVFC.Aspire.Helpers.GcpFirestore.Models;

var builder = DistributedApplication.CreateBuilder(args);

var firestoreConfig = new FirestoreConfig(
    projectId: "test-project");

var firestore = builder.AddGcpFirestore("gcp-firestore")
                       .WithFirestoreConfigs(firestoreConfig)
                       .WithWaitTimeout(15);

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-example")
       .WithReference(firestore)
       .WaitFor(firestore);

await builder.Build().RunAsync();
```

## Emulated Resources Configuration

### `FirestoreConfig`

| Parameter      | Type       | Default | Description                                  |
|----------------|------------|---------|----------------------------------------------|
| `projectId`    | string     | —       | GCP project ID used by Firestore.            |

## Ports

- **HTTP Port:** `8084` *(mapped to internal port `8080` in the container)*

## Provisioning diagram

```mermaid
sequenceDiagram
    participant Aspire as .NET Aspire
    participant Container as Firestore Emulator Container
    participant Configurator as Config Processor
    
    Aspire->>Container: Start container (mtlynch/firestore-emulator)
    Container-->>Aspire: Ready (HTTP port 8084 available)
    Aspire->>Configurator: Trigger OnResourceReady
    Configurator-->>Aspire: Provisioning Completed
    Aspire->>App: Start App with FIRESTORE_EMULATOR_HOST
```

## Public methods

- `AddGcpFirestore` – adds the emulator container.
- `WithFirestoreConfigs` – configures project IDs.
- `WithWaitTimeout` – sets emulator startup delay timeout.
- `WithDockerImage` – replaces the Docker image used by the resource.
- `WithReference` – wires projects to the emulator and sets the `FIRESTORE_EMULATOR_HOST` environment variable automatically.

## Requirements

- .NET 9+
- Aspire.Hosting >= 9.5.0
- Google.Cloud.Firestore >= 3.6.0

## License

Apache-2.0
