# MVFC.Aspire.Helpers.Redis

> 🇧🇷 [Leia em Português](README.pt-BR.md)

Helper for integrating with Redis in .NET Aspire projects, including distributed caching and Redis Commander UI.

## Overview

This project provides extension methods to facilitate integration with Redis in .NET Aspire projects, including distributed caching and Redis Commander UI.

## Project Structure

- [`MVFC.Aspire.Helpers.Redis`](MVFC.Aspire.Helpers.Redis.csproj): Helpers and extensions library for Redis.

## Features

- Adds a configured Redis container.
- Support for Redis Commander UI.
- Support for data persistence via Docker volume (AOF enabled).
- Support for password.

## Compatible Images:
 - `redis`
 - `rediscommander/redis-commander` (UI)

## Installation

```bash
dotnet add package MVFC.Aspire.Helpers.Redis
```

## Usage Example in AppHost

```csharp
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

## Fluent Methods

| Method | Description |
|---|---|
| `WithDockerImage(image, tag)` | Overrides the Docker image used. |
| `WithPassword(password)` | Defines the Redis password. |
| `WithCommander(port?)` | Adds the Redis Commander UI. |
| `WithDataVolume(volumeName)` | Enables persistence with Docker volume (AOF). |

## Main Parameters for `AddRedis`

- `name`: Redis resource name.
- `port` *(Optional)*: Redis Port (default: `6379`).

## Other important Optional parameters:

- **connectionStringSection** (Optional): Defines the path to the environment variable or configuration containing the Redis connection string. Default is `"ConnectionStrings:redis"`. Each `:` indicates a level/section within the `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "redis": "localhost:6379"
  }
}
```

## Port Details

- **Redis Port**: defined via `port` parameter (default: `6379`).
- **Redis Commander Port**: random by default; can be defined via `commanderPort` parameter in `WithCommander`.

## Requirements
- .NET 9+
- Aspire.Hosting >= 9.5.0

## License
Apache-2.0
