# MVFC.Aspire.Helpers.Mailpit

> 🇧🇷 [Leia em Português](README.pt-BR.md)

Helpers for integrating with the MailPit SMTP emulator in .NET Aspire projects, easing the testing of email delivery in development environments.

## Overview

This project allows adding and integrating MailPit as a managed resource in distributed .NET Aspire applications. It simplifies provisioning the MailPit container, exposes the web interface to view received emails, and provides extension methods for AppHost configuration.

## Project Structure

- [`MVFC.Aspire.Helpers.Mailpit`](MVFC.Aspire.Helpers.Mailpit.csproj): Helpers and extensions library for MailPit.

## Features

- Adds the MailPit container to the Aspire application.
- Exposes the web interface for visualizing received emails.
- Extension methods to facilitate AppHost configuration.
- Allows configuration of port, maximum number of messages, authentication, and optional data persistence.

## Compatible Images

- `axllent/mailpit`

## Installation

Add the NuGet package to your AppHost project:

```sh
dotnet add package MVFC.Aspire.Helpers.Mailpit
```

## Usage Example in AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var mailpit = builder.AddMailpit("mailpit")
    .WithMaxMessages(1000)
    .WithDataFilePath("/data/mailpit.db")
    .WithWebAuth(username: "admin", password: "secret");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-example")
       .WithReference(mailpit)
       .WaitFor(mailpit);

await builder.Build().RunAsync();
```

## Fluent Methods

| Method | Description |
|---|---|
| `WithDockerImage(image, tag)` | Overrides the Docker image used. |
| `WithMaxMessages(max)` | Defines the maximum number of stored messages. |
| `WithMaxMessageSize(sizeInMb)` | Defines the maximum size of each message in MB. |
| `WithSmtpAuth()` | Enables SMTP authentication (accept any + insecure). |
| `WithSmtpHostname(hostname)` | Defines the SMTP server hostname. |
| `WithDataFilePath(path)` | Defines the file path for email persistence. |
| `WithWebAuth(username, password)` | Enables authentication on the web interface. |
| `WithVerboseLogging()` | Enables verbose MailPit logs. |

## `AddMailpit` Parameters

| Parameter | Type | Default | Description |
|---|---|---|---|
| `name` | `string` | — | Resource name. |
| `httpPort` | `int` | `8025` | Web interface port. |
| `smtpPort` | `int` | `1025` | SMTP server port. |

## Port Details and Visualization

- **SMTP Port**: defined via `smtpPort` parameter (default: `1025`).
- **Web Port**: defined via `httpPort` parameter (default: `8025`).
- **Interface Access**: The MailPit web interface becomes available at `http://localhost:<httpPort>/`.

## Requirements
- .NET 9+
- Aspire.Hosting >= 9.5.0

## License
Apache-2.0
