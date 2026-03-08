# MVFC.Aspire.Helpers.RabbitMQ

> 🇧🇷 [Leia em Português](README.pt-BR.md)

Helper for integrating with RabbitMQ in .NET Aspire projects, including automatic creation of exchanges, queues, and dead letter queues.

## Overview

MVFC.Aspire.Helpers.RabbitMQ is an extension library for .NET Aspire that facilitates integration with RabbitMQ. It provides extension methods to add RabbitMQ resources to your Aspire application, configure exchanges, queues, and dead letter queues, and setup references between projects.

## Project Structure

- [`MVFC.Aspire.Helpers.RabbitMQ`](MVFC.Aspire.Helpers.RabbitMQ.csproj): Helpers and extensions library for RabbitMQ.

## Features

- Adds a configured RabbitMQ container.
- Support for custom exchanges and queues via `definitions.json`.
- Support for dead letter exchanges (DLX).
- Support for message TTL per queue.
- Support for data persistence via Docker volume.
- Support for custom credentials.
- Support for the RabbitMQ Management UI.

## Compatible Images:
 - `rabbitmq`

## Installation

```bash
dotnet add package MVFC.Aspire.Helpers.RabbitMQ
```

## Usage Example in AppHost

```csharp
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

## Main Parameters for `AddRabbitMQ`

- `name`: RabbitMQ resource name.
- `amqpPort` *(Optional)*: AMQP Port (default: `5672`).
- `httpPort` *(Optional)*: Management UI Port (default: `15672`).

## Fluent Methods

| Method | Description |
|---|---|
| `WithDockerImage(image, tag)` | Overrides the Docker image used. |
| `WithCredentials(username, password)` | Defines username and password. |
| `WithExchanges(exchanges)` | Configures exchanges to be created. |
| `WithQueues(queues)` | Configures queues to be created. |
| `WithDataVolume(volumeName)` | Enables persistence with Docker volume. |

## Exchanges and Queues Configuration

### ExchangeConfig

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Name` | `string` | — | Exchange name. |
| `Type` | `string` | `"direct"` | Type: `direct`, `topic`, `fanout`, `headers`. |
| `Durable` | `bool` | `true` | Durable across restarts. |
| `AutoDelete` | `bool` | `false` | Auto delete when unused. |

### QueueConfig

| Parameter | Type | Default | Description |
|---|---|---|---|
| `Name` | `string` | — | Queue name. |
| `ExchangeName` | `string?` | `null` | Exchange to which the queue will be bound. |
| `RoutingKey` | `string?` | `null` | Binding routing key (default: queue name). |
| `Durable` | `bool` | `true` | Durable across restarts. |
| `AutoDelete` | `bool` | `false` | Auto delete when unused. |
| `DeadLetterExchange` | `string?` | `null` | Dead letter exchange. |
| `MessageTTL` | `int?` | `null` | Message TTL in milliseconds. |

## Other important Optional parameters:

- **connectionStringSection** (Optional): Defines the path to the environment variable or configuration containing the RabbitMQ connection string. Default is `"ConnectionStrings:rabbitmq"`. Each `:` indicates a level/section within the `appsettings.json` file:

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
