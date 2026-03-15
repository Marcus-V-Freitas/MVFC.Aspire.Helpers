# MVFC.Aspire.Helpers.Mailpit

> 🇧🇷 [Leia em Português](README.pt-BR.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.Mailpit)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Mailpit)


Helpers for integrating with the MailPit SMTP emulator in .NET Aspire projects, easing the testing of email delivery in development environments.

## Motivation

Testing e‑mail locally often means:

- Pointing your app to a real SMTP server or a fragile test server.
- No safe way to inspect messages without actually sending them.
- Manual setup of Mailpit/Mailhog containers per project.

With .NET Aspire you can define the container, but you still need to:

- Configure SMTP and web ports.
- Decide where to persist Mailpit data.
- Wire your applications to the SMTP host/port.

`MVFC.Aspire.Helpers.Mailpit` offers:

- `AddMailpit(...)` to provision the SMTP emulator.
- Fluent options for max messages, authentication, data file path and logging.
- `WithReference(...)` to connect projects to Mailpit.

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

```sh
dotnet add package MVFC.Aspire.Helpers.Mailpit
```

## Quick Aspire usage (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.Mailpit;

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

## Fluent methods

| Method                          | Description                                             |
|---------------------------------|---------------------------------------------------------|
| `WithDockerImage(image, tag)`   | Overrides the Docker image used.                        |
| `WithMaxMessages(max)`          | Defines the maximum number of stored messages.          |
| `WithMaxMessageSize(sizeInMb)`  | Defines the maximum size of each message in MB.         |
| `WithSmtpAuth()`                | Enables SMTP authentication (accept any + insecure).    |
| `WithSmtpHostname(hostname)`    | Defines the SMTP server hostname.                       |
| `WithDataFilePath(path)`        | Defines the file path for email persistence.            |
| `WithWebAuth(username, password)` | Enables authentication on the web interface.          |
| `WithVerboseLogging()`          | Enables verbose MailPit logs.                           |

## `AddMailpit` parameters

| Parameter  | Type   | Default | Description        |
|-----------|--------|---------|--------------------|
| `name`    | string | —       | Resource name.     |
| `httpPort`| int    | `8025`  | Web interface port.|
| `smtpPort`| int    | `1025`  | SMTP server port.  |

## Ports and visualization

- **SMTP port**: `smtpPort` (default `1025`).  
- **Web port**: `httpPort` (default `8025`).  
- Web UI: `http://localhost:<httpPort>/`.

## Requirements

- .NET 9+
- Aspire.Hosting >= 9.5.0

## License

Apache-2.0
