# MVFC.Aspire.Helpers.Redis

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.Redis)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Redis)


Helper for integrating with Redis in .NET Aspire projects, including distributed caching and Redis Commander UI.

## Motivation

For local dev, Redis is often set up via an ad‑hoc container:

- No clear place to centralize password configuration.
- No built‑in UI to inspect keys/values.
- No consistent way to mount volumes and preserve state.

With .NET Aspire you can start a Redis container, but you still need to:

- Decide how to expose Redis to your projects.
- Configure Redis Commander (or similar) manually.
- Keep connection strings aligned across services.

`MVFC.Aspire.Helpers.Redis` addresses this by:

- `AddRedis(...)` to provision Redis.
- `WithPassword(...)`, `WithCommander(...)`, `WithDataVolume(...)` to cover common setups.
- `project.WithReference(redis)` to pass the Redis connection string via configuration.

## Overview

This project provides extension methods to facilitate integration with Redis in .NET Aspire projects, including distributed caching and Redis Commander UI.

## Project Structure

- [`MVFC.Aspire.Helpers.Redis`](MVFC.Aspire.Helpers.Redis.csproj): Helpers and extensions library for Redis.

## Features

- Adds a configured Redis container.
- Support for Redis Commander UI.
- Support for data persistence via Docker volume (AOF enabled).
- Support for password.

## Compatible Images

- `redis`
- `rediscommander/redis-commander` (UI)

## Installation

```sh
dotnet add package MVFC.Aspire.Helpers.Redis
```

## Quick Aspire usage (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.Redis;

var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithPassword("my-password")
    .WithCommander()
    .WithDataVolume("redis-data");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-example")
       .WithReference(redis)
       .WaitFor(redis);

await builder.Build().RunAsync();
```

## Provisioning diagram

```mermaid
sequenceDiagram
    participant Aspire as .NET Aspire
    participant Container as Redis Container
    participant UI as Redis Commander
    
    Aspire->>Container: Start container (redis)
    Container-->>Aspire: Ready (port 6379 available)
    Aspire->>UI: Start Redis Commander (if configured)
    Aspire->>App: Start App with Redis ConnectionString
```

## Fluent methods

| Method                         | Description                                            |
|-------------------------------|--------------------------------------------------------|
| `WithDockerImage(image, tag)` | Overrides the Docker image used.                       |
| `WithPassword(password)`      | Defines the Redis password.                            |
| `WithCommander(port?)`        | Adds the Redis Commander UI.                           |
| `WithDataVolume(volumeName)`  | Enables persistence with Docker volume (AOF).          |

## `AddRedis` parameters

- `name`: Redis resource name.  
- `port` *(optional)*: Redis port (default `6379`).

## Other optional parameters

- **`connectionStringSection`** (optional):  
  Path to the configuration section containing the Redis connection string.  
  Default: `"ConnectionStrings:redis"`.

```json
{
  "ConnectionStrings": {
    "redis": "localhost:6379"
  }
}
```

## Port details

- **Redis port**: defined via `port` (default `6379`).  
- **Redis Commander port**: random by default; can be defined via `commanderPort` in `WithCommander`.

## Requirements

- .NET 9+
- Aspire.Hosting >= 9.5.0

## License

Apache-2.0
