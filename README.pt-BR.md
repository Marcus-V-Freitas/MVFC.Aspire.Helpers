# MVFC.Aspire.Helpers

> 🇺🇸 [Read in English](README.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)

Coleção de helpers para integrar serviços comuns com o .NET Aspire de forma rápida e padronizada.

## Motivação

Orquestrar um ambiente local realista para aplicações distribuídas em .NET normalmente significa:

- Escrever e manter vários arquivos `docker-compose`.
- Copiar e colar definições de containers entre projetos.
- Ligar portas, connection strings e health checks na mão.
- Repetir o mesmo setup para cada serviço novo em cada solução nova.

O .NET Aspire resolve parte desse problema ao oferecer um modelo de orquestração em C#, mas você ainda precisa modelar cada dependência de infraestrutura (Mongo, Redis, Keycloak, etc.) sozinho.

Os pacotes do **MVFC.Aspire.Helpers** capturam esse conhecimento uma vez e expõem como pequenos helpers focados:

- Uma linha para adicionar o recurso (`AddXxx`).
- Alguns métodos fluentes para customizar (`WithDataVolume`, `WithDumps`, `WithSeeds`, `WithCommander`, etc.).
- Um único `WithReference(...)` para ligar seus projetos ao recurso com as variáveis de ambiente e dependências corretas.

O objetivo é simples: tornar o ambiente local **o mais próximo possível de produção**, mantendo a experiência de desenvolvimento **clone → run**.

## Padrão das extensões

Todas as bibliotecas seguem a mesma convenção:

- `AddXxx(...)` — registra o recurso de infraestrutura no `IDistributedApplicationBuilder`.
- Métodos fluentes (`WithDataVolume`, `WithDumps`, `WithSeeds`, etc.) — customizam o recurso.
- `project.WithReference(xxx)` — liga um projeto ao recurso, configurando automaticamente:
  - Dependência via `WaitFor`.
  - Variáveis de ambiente (connection strings, base URLs, etc.).
  - Ações de inicialização (ex: executar dumps do Mongo).

Depois que você aprende como um helper funciona, os demais parecem imediatamente familiares.

## Visão Geral

| Pacote | Serviço | Downloads |
|---|---|---|
| [MVFC.Aspire.Helpers.CloudStorage](src/MVFC.Aspire.Helpers.CloudStorage/README.md) | Google Cloud Storage (emulador GCS) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.CloudStorage) |
| [MVFC.Aspire.Helpers.Mongo](src/MVFC.Aspire.Helpers.Mongo/README.md) | MongoDB com Replica Set | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Mongo) |
| [MVFC.Aspire.Helpers.GcpPubSub](src/MVFC.Aspire.Helpers.GcpPubSub/README.md) | Google Pub/Sub (emulador + UI) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.GcpPubSub) |
| [MVFC.Aspire.Helpers.GcpSpanner](src/MVFC.Aspire.Helpers.GcpSpanner/README.md) | Google Cloud Spanner (emulador) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.GcpSpanner) |
| [MVFC.Aspire.Helpers.Gotenberg](src/MVFC.Aspire.Helpers.Gotenberg/README.md) | Gotenberg (conversão de PDF) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Gotenberg) |
| [MVFC.Aspire.Helpers.WireMock](src/MVFC.Aspire.Helpers.WireMock/README.md) | WireMock.Net (mock de APIs) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.WireMock) |
| [MVFC.Aspire.Helpers.Mailpit](src/MVFC.Aspire.Helpers.Mailpit/README.md) | Mailpit (emulador SMTP) | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Mailpit) |
| [MVFC.Aspire.Helpers.RabbitMQ](src/MVFC.Aspire.Helpers.RabbitMQ/README.md) | RabbitMQ | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.RabbitMQ) |
| [MVFC.Aspire.Helpers.Redis](src/MVFC.Aspire.Helpers.Redis/README.md) | Redis + Redis Commander | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Redis) |
| [MVFC.Aspire.Helpers.Keycloak](src/MVFC.Aspire.Helpers.Keycloak/README.md) | Keycloak | ![Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Keycloak) |

---

## Instalação

```sh
dotnet add package MVFC.Aspire.Helpers.CloudStorage
dotnet add package MVFC.Aspire.Helpers.Mongo
dotnet add package MVFC.Aspire.Helpers.GcpPubSub
dotnet add package MVFC.Aspire.Helpers.GcpSpanner
dotnet add package MVFC.Aspire.Helpers.Gotenberg
dotnet add package MVFC.Aspire.Helpers.WireMock
dotnet add package MVFC.Aspire.Helpers.Mailpit
dotnet add package MVFC.Aspire.Helpers.RabbitMQ
dotnet add package MVFC.Aspire.Helpers.Redis
dotnet add package MVFC.Aspire.Helpers.Keycloak
```

---

## Exemplo de Uso

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var cloudStorage = builder.AddCloudStorage("cloud-storage")
    .WithBucketFolder("./bucket-data");

var mongo = builder.AddMongoReplicaSet("mongo")
    .WithDumps(dumps);

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

await builder.Build().RunAsync().ConfigureAwait(false);
```

> Veja o exemplo completo em [playground/](playground/).

---

## Estrutura dos Projetos

```
src/
  MVFC.Aspire.Helpers.CloudStorage/
  MVFC.Aspire.Helpers.GcpPubSub/
  MVFC.Aspire.Helpers.GcpSpanner/
  MVFC.Aspire.Helpers.Gotenberg/
  MVFC.Aspire.Helpers.Keycloak/
  MVFC.Aspire.Helpers.Mailpit/
  MVFC.Aspire.Helpers.Mongo/
  MVFC.Aspire.Helpers.RabbitMQ/
  MVFC.Aspire.Helpers.Redis/
  MVFC.Aspire.Helpers.WireMock/
tests/
  MVFC.Aspire.Helpers.Tests/
playground/
  MVFC.Aspire.Helpers.Playground.Api/
```

---

## Requisitos

- .NET 9 ou .NET 10
- Aspire.Hosting >= 9.5.0
- Docker em execução

---

## Contribuindo

Veja [CONTRIBUTING.md](CONTRIBUTING.md).

## Licença

[Apache-2.0](LICENSE)
