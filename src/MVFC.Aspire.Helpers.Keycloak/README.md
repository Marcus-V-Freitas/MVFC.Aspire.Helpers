# MVFC.Aspire.Helpers.Keycloak

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010%20%7C%2011-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.Keycloak)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Keycloak)


This package provides extension methods for **.NET Aspire**, simplifying the integration, configuration, and initialization of a **Keycloak** server to manage local environment identity and access.

With it, you can not only initialize an official Keycloak container but also inject configurations (like `BaseUrl`, `Realm`, `ClientId`, etc.) and import custom realms (via JSON seeds) at development time.

## Motivation

Calling `WithSeeds` more than once replaces the configured seed list. Calling it with an empty list clears prior seeds.

Keycloak is powerful, but local setup is often painful:

- Large docker commands with many environment variables and import flags.
- Manually importing realms, clients and users from JSON files.
- Repeating the same setup for every project or demo.

With .NET Aspire you can define a Keycloak container, but you still need to:

- Track admin credentials, ports and volumes.
- Decide how to seed realms/clients/users in a repeatable way.
- Make your applications wait for Keycloak to be ready.

`MVFC.Aspire.Helpers.Keycloak` turns this into a small, opinionated API:

- `AddKeycloak(...)` runs Keycloak with sensible defaults for dev.
- `WithAdminCredentials(...)`, `WithDataVolume(...)` and `WithSeeds(...)` centralize Keycloak‑specific concerns.
- `project.WithReference(keycloak, ...)` wires your projects to Keycloak with the correct dependencies and configuration.

## Features and Capabilities

- **Embedded Server:** Starts an official Keycloak container (currently `quay.io/keycloak/keycloak:26.1`).
- **Default Authentication:** Optional configuration with a pre-defined `ClientId` and seeds for developers.  
- **Customizable Port:** Fixed or dynamic port mapping by Aspire.  
- **Transparent Injection:** Adds authentication endpoints to `.WithReference()` projects, configuring URLs, realms, and ClientIds as environment variables.  
- **Persistent Volume:** Optionally saves state and internal databases.  
- **Dynamic Realm Import (Seeds):** Bootstrap your development environment by populating realms, clients, roles, and users via code configuration.

## Installation

Add the package to the main AppHost project in your .NET Aspire solution:

```sh
dotnet add package MVFC.Aspire.Helpers.Keycloak
dotnet add package Aspire.Hosting.Dotnet --version 17.0.0
```

Commands and examples using `AddDotnetProject` target a .NET 10/11 AppHost on Aspire 17. For .NET 9 AppHosts, keep Aspire 13.6.5, omit `Aspire.Hosting.Dotnet` 17 and use `AddProject<Projects.MyApi>("api")`. Adjust the project path to your AppHost.

## Quick Aspire usage (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.Keycloak;
using MVFC.Aspire.Helpers.Keycloak.Configs;
using MVFC.Aspire.Helpers.Keycloak.Seeds;

var builder = DistributedApplication.CreateBuilder(args);

// Adding a basic Keycloak container
var keycloak = builder.AddKeycloak("keycloak", httpPort: 8081)
    .WithAdminCredentials("admin", "admin")
    .WithSeeds(new MyAppRealm());

var api = builder.AddDotnetProject("api", "../My_Api/My_Api.csproj")
    .WithReference(keycloak, realmName: "my-app-realm", clientId: "my-api", clientSecret: "api-secret-1234")
    .WaitFor(keycloak);

await builder.Build().RunAsync();

internal sealed class MyAppRealm : IKeycloakRealmSeed
{
    public string RealmName => "my-app-realm";

    public IReadOnlyList<KeycloakClientConfig> Clients =>
    [
        new(ClientId: "my-api", ClientSecret: "api-secret-1234",
            DirectAccessGrantsEnabled: true, AddAudienceMapper: true),
    ];
}
```

## Using the injected configuration in the API

By default, when using `.WithReference(keycloak, realmName, clientId, clientSecret)`, Aspire injects the following configuration keys into your `IConfiguration`:

| Configuration Key       | Description                                   | Example                 |
| ----------------------- | --------------------------------------------- | ----------------------- |
| `Keycloak:BaseUrl`      | Full URL to access the Keycloak container.    | `http://localhost:8081` |
| `Keycloak:Realm`        | Realm name propagated from the AppHost.       | `my-app-realm`          |
| `Keycloak:ClientId`     | OAuth/OpenID client ID.                       | `my-api`                |
| `Keycloak:ClientSecret` | (Optional) Secret associated with the client. | `api-secret-1234`       |

Example in your API:

```csharp
var baseUrl  = builder.Configuration["Keycloak:BaseUrl"];
var realm    = builder.Configuration["Keycloak:Realm"];
var clientId = builder.Configuration["Keycloak:ClientId"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // dev
        options.Audience = clientId;
        options.MetadataAddress = $"{baseUrl}/realms/{realm}/.well-known/openid-configuration";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = $"{baseUrl}/realms/{realm}",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = clientId
        };
    });

builder.Services.AddAuthorization();
```

## Provisioning diagram

```mermaid
sequenceDiagram
    participant Aspire as .NET Aspire
    participant Container as Keycloak Container
    participant App as Consumer

    Aspire->>Aspire: BeforeStartEvent generates realm JSON files from IKeycloakRealmSeed
    Aspire->>Container: Start Keycloak with import files and --import-realm
    Container->>Container: Import configured realms, clients, users and roles
    Aspire->>Container: Probe management endpoint /health/ready
    Container-->>Aspire: HTTP health check passes
    Aspire->>App: Start consumer with Keycloak settings (WaitFor)
```

## `AddKeycloak()` options

| Parameter | Type | Description | Default |
| --- | --- | --- | --- |
| `name` | string | Required resource name in Aspire. | — |
| `httpPort` | int | HTTP port exposed to consumers. | `8081` |
| `managementPort` | int | Management port used by the `/health/ready` check. | `9000` |

## Additional extensions

- `.WithAdminCredentials(username, password)` – sets administrator credentials (local default: `admin`/`admin`).
- `.WithSeeds(seeds)` – accepts `IKeycloakRealmSeed` implementations, generates JSON files before startup and enables `--import-realm`.
- `.WithImportStrategy(strategy)` – selects the realm import strategy.
- `.WithDataVolume(volumeName)` – persists data in a named Docker volume.

## Requirements

- .NET 9, 10 or 11
- Aspire.Hosting 13.6.5 on .NET 9; Aspire.Hosting 17.0.0 on .NET 10/11. AppHost examples use `Aspire.Hosting.Dotnet` 17 and `AddDotnetProject`; .NET 9 AppHosts retain the `ProjectResource` model.
- Docker running locally

## License

Apache-2.0
