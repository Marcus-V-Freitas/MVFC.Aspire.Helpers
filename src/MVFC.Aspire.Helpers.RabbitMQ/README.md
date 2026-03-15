# MVFC.Aspire.Helpers.RabbitMQ

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.RabbitMQ)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.RabbitMQ)


Helper for integrating with RabbitMQ in .NET Aspire projects, including automatic creation of exchanges, queues, and dead letter queues.

## Motivation

Using RabbitMQ in local environments often requires:

- Maintaining a `definitions.json` with exchanges/queues/bindings.
- Manually loading that file or running `rabbitmqctl` commands.
- Repeating configuration for each Aspire solution.

With .NET Aspire you can start a RabbitMQ container, but you still need to:

- Configure users/passwords and management UI.
- Declare exchanges, queues and DLQs consistently.
- Keep connection details in sync with your applications.

`MVFC.Aspire.Helpers.RabbitMQ` wraps this into a fluent API:

- `AddRabbitMQ(...)` to provision RabbitMQ in Aspire.
- `WithExchanges(...)` and `WithQueues(...)` to declare topology from code.
- `WithDataVolume(...)` and `WithCredentials(...)` for persistence and security.
- `project.WithReference(rabbitMQ)` to wire your services to the broker.

## Overview

MVFC.Aspire.Helpers.RabbitMQ is an extension library for .NET Aspire that facilitates integration with RabbitMQ. It provides extension methods to add RabbitMQ resources to your Aspire application, configure exchanges, queues, and dead letter queues, and setup references between projects.

## Project Structure

- [`MVFC.Aspire.Helpers.RabbitMQ`](MVFC.Aspire.Helpers.RabbitMQ.csproj): Helpers and extensions library for RabbitMQ.

## Features

- Adds a configured RabbitMQ container.
- Support for custom exchanges and queues via `ExchangeConfig`/`QueueConfig`.
- Support for dead letter exchanges (DLX).
- Support for message TTL per queue.
- Support for data persistence via Docker volume.
- Support for custom credentials.
- Support for the RabbitMQ Management UI.

## Compatible Images

- `rabbitmq`

## Installation

```sh
dotnet add package MVFC.Aspire.Helpers.RabbitMQ
```

## Quick Aspire usage (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.RabbitMQ;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitMQ = builder.AddRabbitMQ("rabbitmq")
    .WithCredentials(username: "admin", password: "password")
    .WithExchanges([
        new ExchangeConfig("test-exchange", "topic"),
        new ExchangeConfig("dead-letter", "fanout")
    ])
    .WithQueues([
        new QueueConfig("test-queue", ExchangeName: "test-exchange", RoutingKey: "test.*", DeadLetterExchange: "dead-letter"),
        new QueueConfig("empty-queue", ExchangeName: "test-exchange", RoutingKey: "empty.*"),
        new QueueConfig("dlq", ExchangeName: "dead-letter")
    ])
    .WithDataVolume("rabbit-mq");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-example")
       .WithReference(rabbitMQ)
       .WaitFor(rabbitMQ);

await builder.Build().RunAsync();
```

## `AddRabbitMQ` parameters

- `name`: RabbitMQ resource name.  
- `amqpPort` *(optional)*: AMQP port (default `5672`).  
- `httpPort` *(optional)*: Management UI port (default `15672`).

## Fluent methods

| Method                        | Description                               |
|------------------------------|-------------------------------------------|
| `WithDockerImage(image, tag)`| Overrides the Docker image used.          |
| `WithCredentials(username, password)` | Defines username and password.   |
| `WithExchanges(exchanges)`   | Configures exchanges to be created.       |
| `WithQueues(queues)`         | Configures queues to be created.          |
| `WithDataVolume(volumeName)` | Enables persistence with Docker volume.   |

## Exchanges and queues configuration

### `ExchangeConfig`

| Parameter   | Type   | Default  | Description                                              |
|------------|--------|----------|----------------------------------------------------------|
| `Name`     | string | —        | Exchange name.                                           |
| `Type`     | string | `"direct"` | Type: `direct`, `topic`, `fanout`, `headers`.         |
| `Durable`  | bool   | `true`   | Durable across restarts.                                |
| `AutoDelete` | bool | `false`  | Auto delete when unused.                                |

### `QueueConfig`

| Parameter          | Type    | Default | Description                                  |
|-------------------|---------|---------|----------------------------------------------|
| `Name`            | string  | —       | Queue name.                                  |
| `ExchangeName`    | string? | `null`  | Exchange to which the queue will be bound.   |
| `RoutingKey`      | string? | `null`  | Binding routing key (default: queue name).   |
| `Durable`         | bool    | `true`  | Durable across restarts.                     |
| `AutoDelete`      | bool    | `false` | Auto delete when unused.                     |
| `DeadLetterExchange` | string? | `null`| Dead letter exchange.                        |
| `MessageTTL`      | int?    | `null`  | Message TTL in milliseconds.                 |

## Other optional parameters

- **`connectionStringSection`** (optional):  
  Path to the configuration section containing the RabbitMQ connection string.  
  Default: `"ConnectionStrings:rabbitmq"`.

```json
{
  "ConnectionStrings": {
    "rabbitmq": "localhost:5672"
  }
}
```

## Requirements

- .NET 9+
- Aspire.Hosting >= 9.5.0

## License

Apache-2.0
