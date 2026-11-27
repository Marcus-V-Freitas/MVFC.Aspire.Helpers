# MVFC.Aspire.Helpers.Mongo

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010%20%7C%2011-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.Mongo)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Mongo)


Helpers for integrating with MongoDB in .NET Aspire projects, including support for Replica Sets and automatic initialization.

## Motivation

Opt into writable-primary readiness with `builder.AddMongoReplicaSet("mongo").WithPrimaryHealthCheck()`, and add `consumer.WaitFor(mongo)` explicitly. Each probe has a three-second deadline and reuses its MongoDB client. This verifies `hello.isWritablePrimary`; it does not wait for dumps, which retain their existing consumer-ready lifecycle. Without this fluent option, startup behavior is unchanged.

Setting up MongoDB locally is easy; setting up a **Replica Set with realistic behavior and seed data** is not.

Typical pain points in local dev:

- Creating and initializing the Replica Set using custom scripts.
- Remembering which ports are exposed and how to build the connection string.
- Manually running `mongorestore` or ad‑hoc scripts every time you start the environment.

With .NET Aspire you can already model a MongoDB container, but you still have to wire:

- Replica Set arguments and init scripts.
- Data volumes for persistence between runs.
- Connection string configuration for your projects.
- Any logic that runs once to populate sample data.

`MVFC.Aspire.Helpers.Mongo` wraps these concerns into a focused API:

- `AddMongoReplicaSet(...)` to get a ready‑to‑use Replica Set container.
- Fluent methods like `WithDumps(...)` and `WithDataVolume(...)`.
- `project.WithReference(mongo)` to inject the connection string and register automatic dump execution.

## Overview

This project facilitates the configuration and integration of MongoDB in distributed .NET Aspire applications, providing extension methods to:

- Add a MongoDB container configured as a Replica Set.
- Automatically initialize the Replica Set via script.
- Populate the database with sample data using custom dumps.

### Why use a Replica Set?

MongoDB only allows multi‑document transactions when configured as a Replica Set, even in local environments.

By using the helper with Replica Set, you can:

- **Simulate local transactions:**  
  Test transactional operations (commit/rollback) across multiple documents and collections, matching your production behavior.
- **Prepare for high availability:**  
  A Replica Set is the foundation for advanced MongoDB features like failover and redundancy; even locally, it helps you run closer to a real‑world setup.

## Project structure

- [`MVFC.Aspire.Helpers.Mongo`](MVFC.Aspire.Helpers.Mongo.csproj): Helpers and extensions library for MongoDB.

## Features

- Adds a MongoDB container configured as a Replica Set.
- Automatic Replica Set initialization.
- Support for populating collections with sample data.
- Extension methods to simplify AppHost configuration.

### Compatible Docker images

- `mongo`

## Installation

Add the NuGet package to your AppHost project:

```sh
dotnet add package MVFC.Aspire.Helpers.Mongo
```

## Quick Aspire usage (AppHost)

Minimal example in your `AppHost` using Bogus to seed test data:

```csharp
using Aspire.Hosting;
using Bogus;
using MVFC.Aspire.Helpers.Mongo;
using MVFC.Aspire.Helpers.Mongo.Models;

var builder = DistributedApplication.CreateBuilder(args);

IReadOnlyCollection<IMongoClassDump> dumps =
[
    new MongoClassDump<TestDatabase>(
        DatabaseName: "TestDatabase",
        CollectionName: "TestCollection",
        Quantity: 100,
        Faker: new Faker<TestDatabase>()
            .CustomInstantiator(f => new TestDatabase(
                f.Person.FirstName,
                f.Person.Cpf())))
];

var mongo = builder.AddMongoReplicaSet("mongo")
    .WithPrimaryHealthCheck()
    .WithDataVolume("mongo-data");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-example")
       .WithReference(mongo)
       .WaitForDumps(mongo);

mongo.WithDumps(dumps);

await builder.Build().RunAsync();
```

You can configure dumps, references and `WaitForDumps` in any order during AppHost configuration. `WaitForDumps(mongo)` adds primary readiness and waits for shared dump completion before starting the consumer. Concurrent consumers share the result; a failed or canceled execution is not automatically replayed. Canceling a waiting consumer does not cancel shared work started by the wait; AppHost shutdown does.

Without `WaitForDumps`, `WithReference(mongo)` retains the existing behavior: dumps run when the project from the first reference becomes ready. The sequence diagram below describes that legacy flow. Use the explicit wait when seeds are required during startup. See [runtime contracts](../../docs/runtime-contracts.md) for partial writes and persistence constraints.

What this setup gives you:

- A MongoDB Replica Set running in Docker.
- A persistent Docker volume `mongo-data` (if configured).
- Database/collection automatically populated with fake data at startup.
- A connection string injected into the project configuration.

## Provisioning diagram

```mermaid
sequenceDiagram
    participant Aspire as .NET Aspire
    participant Container as MongoDB Container
    participant Project as First referencing project
    participant Executor as Mongo dump executor

    Aspire->>Container: Start container (mongo)
    Container->>Container: Run Replica Set initialization script on first initialization
    Container-->>Aspire: Resource ready
    Aspire->>Project: Start project with the connection string (WaitFor in this example)
    Project-->>Aspire: Project ready
    Aspire->>Executor: Dispatch the project's ResourceReadyEvent
    Executor->>Container: Execute configured dumps
```

## Fluent methods

| Method                        | Description                                         |
| ----------------------------- | --------------------------------------------------- |
| `WithDockerImage(image, tag)` | Overrides the Docker image used.                    |
| `WithDumps(dumps)`            | Configures data dumps to execute on initialization. |
| `WithPrimaryHealthCheck()` | Opts into writable-primary checks with a three-second probe deadline; consumers still need `WaitFor(mongo)`. |
| `WithDataVolume(volumeName)`  | Enables persistence with a Docker volume.           |

## Populating sample data

`MongoClassDump<T>` is a class used to facilitate the automatic insertion of sample data into MongoDB collections during environment initialization. It acts as a **template** to populate the database with fake documents, useful for local testing and development.

**Main parameters:**

- `DatabaseName`: Database name.
- `CollectionName`: Collection name.
- `Quantity`: Number of documents.
- `Faker`: Data generator (for example, using the **Bogus** library with the `Faker` class).

## Configuration options

### Optional parameters

- **`volumeName`** (optional):  
  Local Docker volume name used to persist data between debugging sessions.  
  Default: `null` (volume discarded between executions).

- **`connectionStringSection`** (optional):  
  Path to the configuration section containing the MongoDB connection string.  
  Default: `"ConnectionStrings:mongo"`.

Each `:` indicates a level/section within the `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "mongo": "mongodb://localhost:27017/"
  }
}
```

### Visualization and ports

- **Port used:** `27017` (MongoDB default).
- **View databases:**  
  Connect via a MongoDB client (MongoDB Compass, Robo 3T, `mongosh`, etc.) using:

  `mongodb://localhost:27017/`

## Default image

The default image is pinned to a tested SHA256 digest. Select another image/tag with `WithDockerImage(image, tag)` or Aspire image methods; the override clears the previous digest. See [runtime contracts](../../docs/runtime-contracts.md) for image maintenance, readiness and emulator limitations.

## Requirements

- .NET 9, 10 or 11
- Aspire.Hosting >= 13.6.5
- Docker running locally
- Bogus >= 35.6.0
- MongoDB.Driver >= 3.11.1

## License

Apache-2.0

## Repeatable seeds

`MongoClassDump<T>` retains its append behavior. Opt into `MongoUpsertDump<T>` through the same `WithDumps` API to replace or insert documents by their serialized `_id`:

```csharp
var dump = new MongoUpsertDump<BsonDocument>(
    DatabaseName: "catalog",
    CollectionName: "items",
    Quantity: 1,
    Faker: new Faker<BsonDocument>().CustomInstantiator(_ => new BsonDocument
    {
        ["_id"] = "default-item",
        ["name"] = "Default item",
    }));

mongo.WithDumps(dump);
```

Supply stable, non-null identities. Missing/null IDs and duplicate IDs within a batch are rejected before writing. Reexecution replaces the complete document; it does not delete documents absent from the batch. A canceled or failed batch may leave partial writes; rerun with the same IDs to reconcile. MongoDB's unique `_id` index prevents duplicate identities; other unique constraints can still reject writes. Concurrent updates to the same ID have last-writer semantics.

Client caching compares complete connection strings using ordinal case-sensitive identity, preserving differences in passwords and database names.
