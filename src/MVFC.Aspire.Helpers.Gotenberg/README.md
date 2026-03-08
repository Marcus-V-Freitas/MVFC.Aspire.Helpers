# MVFC.Aspire.Helpers.Gotenberg

> 🇧🇷 [Leia em Português](README.pt-BR.md)

Helpers for integrating with Gotenberg (API for converting documents and PDFs) in .NET Aspire projects.

## Overview

This project allows you to easily add and integrate Gotenberg as a managed resource in distributed .NET Aspire applications. It simplifies provisioning the Gotenberg container and provides extension methods for configuring it in the AppHost.

## Project Structure

- [`MVFC.Aspire.Helpers.Gotenberg`](MVFC.Aspire.Helpers.Gotenberg.csproj): Helpers and extensions library for Gotenberg.

## Features

- Adds the Gotenberg container to the Aspire application.
- Manages automatic health check at `/health`.
- Automatically injects the service's base URL into the consuming project.
- Extension methods for quick configuration of custom ports.

## Compatible Images

- `gotenberg/gotenberg` (default tag `8`)

## Installation

Add the NuGet package to your AppHost project:

```sh
dotnet add package MVFC.Aspire.Helpers.Gotenberg
```

## Usage Example in AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Adds the Gotenberg container on host port 3000
var gotenberg = builder.AddGotenberg("gotenberg", port: 3000);

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-example")
       .WithReference(gotenberg)
       .WaitFor(gotenberg);

await builder.Build().RunAsync();
```

## Reference in Backend Project (Api, Web, etc)

When using `.WithReference(gotenberg)`, the AppHost will automatically inject an environment variable containing the accessible Gotenberg address so the consuming application can connect:

`GOTENBERG__BASE_URL` = `http://localhost:<port>`

## Fluent Methods

| Method | Description |
|---|---|
| `WithDockerImage(image, tag)` | Overrides the Docker image or its tag used. |

## `AddGotenberg` Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `name` | `string` | — | Resource name in Aspire. |
| `port` | `int` | `3000` | HTTP port exposed on the host for communication. |

## Port Details and Visualization

- The default mapped port is `3000`. Internally, the container also runs on `3000`.
- An HTTP health check is automatically registered pointing to `http://localhost:<port>/health`.

## Requirements
- .NET 9+
- Aspire.Hosting >= 9.5.0

## License
Apache-2.0
