# MVFC.Aspire.Helpers.Mongo

> 🇧🇷 [Leia em Português](README.pt-BR.md)

Helpers for integrating with MongoDB in .NET Aspire projects, including support for Replica Sets and automatic initialization.

## Overview

This project facilitates the configuration and integration of MongoDB in distributed .NET Aspire applications, providing extension methods to:

- Add a MongoDB container configured as a Replica Set.
- Automatically initialize the Replica Set via script.
- Populate the database with sample data using custom dumps.

## Why use a Replica Set?

MongoDB only allows multi-document transactions when configured as a Replica Set, even in local environments.
By using the helper with Replica Set, you can:

- **Simulate local transactions:**
  It allows testing transaction operations (commit/rollback) across multiple documents and collections, equal to the production environment.
- **High availability and fault tolerance:**
  A Replica Set is the foundation for advanced MongoDB features like failover and redundancy (even if local, it prepares the environment).

## Project Structure

- [`MVFC.Aspire.Helpers.Mongo`](MVFC.Aspire.Helpers.Mongo.csproj): Helpers and extensions library for MongoDB.

## Features

- Adds a MongoDB container with a Replica Set.
- Automatic Replica Set initialization.
- Support for populating collections with sample data.
- Extension methods to facilitate AppHost configuration.

## Compatible Images:
 - `mongo`

## Installation

Add the NuGet package to your AppHost project:

```sh
dotnet add package MVFC.Aspire.Helpers.Mongo
```

## Usage Example in AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

IReadOnlyCollection<IMongoClassDump> dumps = [
    new MongoClassDump<TestDatabase>(
        DatabaseName: "TestDatabase",
        CollectionName: "TestCollection",
        Quantity: 100,
        Faker: new Faker<TestDatabase>()
              .CustomInstantiator(f => new TestDatabase(f.Person.FirstName, f.Person.Cpf())))
];

var mongo = builder.AddMongoReplicaSet("mongo")
    .WithDumps(dumps)
    .WithDataVolume("mongo-data");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-example")
       .WithReference(mongo)
       .WaitFor(mongo);

await builder.Build().RunAsync();
```

## Fluent Methods

| Method | Description |
|---|---|
| `WithDockerImage(image, tag)` | Overrides the Docker image used. |
| `WithDumps(dumps)` | Configures data dumps to execute upon initialization. |
| `WithDataVolume(volumeName)` | Enables persistence with Docker volume. |

## Populating sample data

`MongoClassDump<T>` is a class used to facilitate the automatic insertion of sample data into MongoDB collections during environment initialization. It serves as a "template" to populate the database with fictitious documents, useful for local testing and development.

**Main Parameters:**
- `DatabaseName`: Database name.
- `CollectionName`: Collection name.
- `Quantity`: Amount of documents.
- `Faker`: Data generator (e.g., using the **Bogus** library with the **Faker** class).

## Other important Optional parameters:

- **volumeName** (Optional): Represents the local docker volume name to persist data between debugging sessions. Default is null (volume discarded between executions).
- **connectionStringSection** (Optional): Defines the path to the environment variable or configuration containing the MongoDB connection string. Default is `"ConnectionStrings:mongo"`. Each `:` indicates a level/section within the `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "mongo": "mongodb://localhost:27017/"
  }
}
```

## MongoDB Visualization and Port Details

- **Port used:** `27017` (MongoDB default).
- **View databases:**
  Connect via a MongoDB client (e.g., MongoDB Compass, Robo 3T, mongosh) using:
  `mongodb://localhost:27017/`

## Requirements
- .NET 9+
- Aspire.Hosting >= 9.5.0
- Bogus >= 35.6.0
- MongoDB.Driver >= 3.5.0

## License
Apache-2.0
