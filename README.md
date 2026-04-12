# MVFC.Aspire.Helpers

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)

A collection of helpers to integrate common services with .NET Aspire quickly and in a standardized way.

> 🏢 **Enterprise Validation: Featured on GFT Technologies**
> 
> **[Read the full article on the GFT Engineering Blog](https://www.gft.com/br/pt/blog/net-aspire-na-pratica-simplificando-a-orquestracao)**
> 
> *`MVFC.Aspire.Helpers` was built to solve real local orchestration challenges at an enterprise scale. Featured by a global powerhouse in technology, this article details how the library simplifies configuring services like MongoDB, RabbitMQ, and Keycloak. Discover how these abstractions reduce developer cognitive load and enable Shift-Left Testing in complex environments.*

## Motivation

Orchestrating a realistic local environment for .NET distributed applications usually means:

- Writing and maintaining multiple `docker-compose` files.
- Copy‑pasting container definitions between projects.
- Manually wiring ports, connection strings and health checks.
- Repeating the same setup for every new service in every new solution.

.NET Aspire solves part of this problem by giving you a first‑class orchestration model in C#, but you still have to model each infrastructure dependency (Mongo, Redis, Keycloak, etc.) yourself.

**MVFC.Aspire.Helpers** packages capture this knowledge once and expose it as small, focused helpers:

- One line to add the resource (`AddXxx`).
- A few fluent methods to customize it (`WithDataVolume`, `WithDumps`, `WithSeeds`, `WithCommander`, etc.).
- A single `WithReference(...)` call to link your projects to the resource with the right environment variables and dependencies.

The goal is simple: make your local environment **as close as possible to production** while keeping the developer experience **clone → run**.

## Extension pattern

All libraries follow the same convention:

- `AddXxx(...)` — registers the infrastructure resource in the `IDistributedApplicationBuilder`.
- Fluent methods (`WithDataVolume`, `WithDumps`, `WithSeeds`, etc.) — customize the resource.
- `project.WithReference(xxx)` — links a project to the resource, automatically configuring:
  - `WaitFor` dependency.
  - Environment variables (connection strings, base URLs, etc.).
  - Initialization actions (e.g. executing Mongo dumps).

Once you learn how one helper works, the others feel immediately familiar.

## Overview

| Package | Service | Downloads |
|---|---|---|
| [MVFC.Aspire.Helpers.CloudStorage](src/MVFC.Aspire.Helpers.CloudStorage/README.md) | Google Cloud Storage (GCS emulator) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.CloudStorage) |
| [MVFC.Aspire.Helpers.Mongo](src/MVFC.Aspire.Helpers.Mongo/README.md) | MongoDB with Replica Set | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Mongo) |
| [MVFC.Aspire.Helpers.GcpFirestore](src/MVFC.Aspire.Helpers.GcpFirestore/README.md) | Google Cloud Firestore (emulator) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.GcpFirestore) |
| [MVFC.Aspire.Helpers.GcpPubSub](src/MVFC.Aspire.Helpers.GcpPubSub/README.md) | Google Pub/Sub (emulator + UI) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.GcpPubSub) |
| [MVFC.Aspire.Helpers.GcpSpanner](src/MVFC.Aspire.Helpers.GcpSpanner/README.md) | Google Cloud Spanner (emulator) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.GcpSpanner) |
| [MVFC.Aspire.Helpers.Gotenberg](src/MVFC.Aspire.Helpers.Gotenberg/README.md) | Gotenberg (PDF conversion) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Gotenberg) |
| [MVFC.Aspire.Helpers.WireMock](src/MVFC.Aspire.Helpers.WireMock/README.md) | WireMock.Net (API mocking) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.WireMock) |
| [MVFC.Aspire.Helpers.Mailpit](src/MVFC.Aspire.Helpers.Mailpit/README.md) | Mailpit (SMTP emulator) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Mailpit) |
| [MVFC.Aspire.Helpers.RabbitMQ](src/MVFC.Aspire.Helpers.RabbitMQ/README.md) | RabbitMQ | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.RabbitMQ) |
| [MVFC.Aspire.Helpers.Redis](src/MVFC.Aspire.Helpers.Redis/README.md) | Redis + Redis Commander | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Redis) |
| [MVFC.Aspire.Helpers.Keycloak](src/MVFC.Aspire.Helpers.Keycloak/README.md) | Keycloak | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Keycloak) |
| [MVFC.Aspire.Helpers.ApigeeEmulator](src/MVFC.Aspire.Helpers.ApigeeEmulator/README.md) | Apigee Emulator (API proxy) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.ApigeeEmulator) |

---

## Installation

```sh
dotnet add package MVFC.Aspire.Helpers.CloudStorage
dotnet add package MVFC.Aspire.Helpers.Mongo
dotnet add package MVFC.Aspire.Helpers.GcpFirestore
dotnet add package MVFC.Aspire.Helpers.GcpPubSub
dotnet add package MVFC.Aspire.Helpers.GcpSpanner
dotnet add package MVFC.Aspire.Helpers.Gotenberg
dotnet add package MVFC.Aspire.Helpers.WireMock
dotnet add package MVFC.Aspire.Helpers.Mailpit
dotnet add package MVFC.Aspire.Helpers.RabbitMQ
dotnet add package MVFC.Aspire.Helpers.Redis
dotnet add package MVFC.Aspire.Helpers.Keycloak
dotnet add package MVFC.Aspire.Helpers.ApigeeEmulator
```

---

## Usage Example

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var cloudStorage = builder.AddCloudStorage("cloud-storage")
    .WithBucketFolder("./bucket-data");

var mongo = builder.AddMongoReplicaSet("mongo")
    .WithDumps(dumps);

var firestore = builder.AddGcpFirestore("gcp-firestore")
    .WithFirestoreConfigs(firestoreConfig);

var pubSub = builder.AddGcpPubSub("gcp-pubsub")
    .WithPubSubConfigs(pubSubConfig);

var spanner = builder.AddGcpSpanner("spanner")
    .WithSpannerConfigs(spannerConfig);

var mailpit = builder.AddMailpit("mailpit");

var rabbitMQ = builder.AddRabbitMQ("rabbitmq")
    .WithExchanges([new ExchangeConfig("test-exchange", "topic")])
    .WithQueues([new QueueConfig(Name: "test-queue", ExchangeName: "test-exchange", RoutingKey: "test.*")])
    .WithDataVolume("rabbit-mq");

var redis = builder.AddRedis("redis")
    .WithCommander()
    .WithDataVolume("redis-data");

var gotenberg = builder.AddGotenberg("gotenberg", port: 3000);

var keycloak = builder.AddKeycloak("keycloak")
    .WithAdminCredentials("admin", "Admin@123")
    .WithSeeds([new MyAppRealm()])
    .WithDataVolume("key-cloak-data");

var wireMock = builder.AddWireMock("wireMock", port: 7070, configure: (server) => {
    server.Endpoint("/api/test")
          .WithDefaultBodyType(BodyType.String)
          .OnGet<string>(() => ("OK", HttpStatusCode.OK, null));
});

var api = builder.AddProject<Projects.MyApi>("api");

var apigeeWorkspace = Path.Combine(Directory.GetCurrentDirectory(), "apigee-workspace");

var apigee = builder.AddApigeeEmulator("apigee-emulator")
    .WithWorkspace(apigeeWorkspace, "health")
    .WithEnvironment("local")
    .WithBackend(api, "origin");

await builder.Build().RunAsync().ConfigureAwait(false);
```

> See [playground/](playground/) for the full working example.

---

## Project Structure

```
src/
  MVFC.Aspire.Helpers.CloudStorage/
  MVFC.Aspire.Helpers.GcpFirestore/
  MVFC.Aspire.Helpers.GcpPubSub/
  MVFC.Aspire.Helpers.GcpSpanner/
  MVFC.Aspire.Helpers.Gotenberg/
  MVFC.Aspire.Helpers.Keycloak/
  MVFC.Aspire.Helpers.Mailpit/
  MVFC.Aspire.Helpers.Mongo/
  MVFC.Aspire.Helpers.RabbitMQ/
  MVFC.Aspire.Helpers.Redis/
  MVFC.Aspire.Helpers.WireMock/
  MVFC.Aspire.Helpers.ApigeeEmulator/
tests/
  MVFC.Aspire.Helpers.Tests/
playground/
  MVFC.Aspire.Helpers.Playground.Api/
```

---

## Requirements

- .NET 9 or .NET 10
- Aspire.Hosting >= 9.5.0
- Docker running locally

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[Apache-2.0](LICENSE)
