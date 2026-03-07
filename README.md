# MVFC.Aspire.Helpers

Coleção de helpers para integrar serviços comuns com o .NET Aspire de forma rápida e padronizada.

## Visão Geral

Este repositório fornece extensões de configuração do Aspire para os seguintes serviços:

| Pacote | Serviço | NuGet |
|---|---|---|
| [MVFC.Aspire.Helpers.CloudStorage](src/MVFC.Aspire.Helpers.CloudStorage/README.md) | Google Cloud Storage (emulador GCS) | `MVFC.Aspire.Helpers.CloudStorage` |
| [MVFC.Aspire.Helpers.Mongo](src/MVFC.Aspire.Helpers.Mongo/README.md) | MongoDB com Replica Set | `MVFC.Aspire.Helpers.Mongo` |
| [MVFC.Aspire.Helpers.GcpPubSub](src/MVFC.Aspire.Helpers.GcpPubSub/README.md) | Google Pub/Sub (emulador + UI) | `MVFC.Aspire.Helpers.GcpPubSub` |
| [MVFC.Aspire.Helpers.Gotenberg](src/MVFC.Aspire.Helpers.Gotenberg/README.md) | Gotenberg (conversão de PDF) | `MVFC.Aspire.Helpers.Gotenberg` |
| [MVFC.Aspire.Helpers.WireMock](src/MVFC.Aspire.Helpers.WireMock/README.md) | WireMock.Net (mock de APIs) | `MVFC.Aspire.Helpers.WireMock` |
| [MVFC.Aspire.Helpers.Mailpit](src/MVFC.Aspire.Helpers.Mailpit/README.md) | Mailpit (emulador SMTP) | `MVFC.Aspire.Helpers.Mailpit` |
| [MVFC.Aspire.Helpers.RabbitMQ](src/MVFC.Aspire.Helpers.RabbitMQ/README.md) | RabbitMQ | `MVFC.Aspire.Helpers.RabbitMQ` |
| [MVFC.Aspire.Helpers.Redis](src/MVFC.Aspire.Helpers.Redis/README.md) | Redis + Redis Commander | `MVFC.Aspire.Helpers.Redis` |

---

## Instalação

```sh
dotnet add package MVFC.Aspire.Helpers.CloudStorage
dotnet add package MVFC.Aspire.Helpers.Mongo
dotnet add package MVFC.Aspire.Helpers.GcpPubSub
dotnet add package MVFC.Aspire.Helpers.Gotenberg
dotnet add package MVFC.Aspire.Helpers.WireMock
dotnet add package MVFC.Aspire.Helpers.Mailpit
dotnet add package MVFC.Aspire.Helpers.RabbitMQ
dotnet add package MVFC.Aspire.Helpers.Redis
```

---

## Exemplo de Uso no AppHost

O exemplo abaixo demonstra como configurar todos os helpers em um único `AppHost`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// --- Cloud Storage ---
var cloudStorage = builder.AddCloudStorage("cloud-storage")
    .WithBucketFolder("./bucket-data");

// --- MongoDB Replica Set ---
var mongo = builder.AddMongoReplicaSet("mongo")
    .WithDumps([/* lista de IMongoClassDump */])
    .WithDataVolume("mongo-data");

// --- GCP Pub/Sub ---
var pubSubConfig = new PubSubConfig("my-project", new MessageConfig(
    TopicName: "my-topic",
    SubscriptionName: "my-subscription",
    PushEndpoint: "/api/pubsub"));

var gcpPubSub = builder.AddGcpPubSub("gcp-pubsub")
    .WithPubSubConfigs([pubSubConfig]);

var pubSubUI = builder.AddGcpPubSubUI("pubsub-ui")
    .WithReference(gcpPubSub)
    .WaitFor(gcpPubSub);

// --- Gotenberg ---
var gotenberg = builder.AddGotenberg("gotenberg", port: 3000);

// --- WireMock ---
var wireMock = builder.AddWireMock("wire-mock", port: 8090, configure: server =>
{
    server.Endpoint("/api/test")
          .WithDefaultBodyType(BodyType.Json)
          .OnGet<MyModel>(() => (new MyModel(), HttpStatusCode.OK, null));
});

// --- Mailpit ---
var mailpit = builder.AddMailpit("mailpit")
    .WithMaxMessages(500);

// --- RabbitMQ ---
var rabbitMQ = builder.AddRabbitMQ("rabbitmq")
    .WithCredentials("admin", "password")
    .WithExchanges([new ExchangeConfig("orders", "topic")])
    .WithQueues([new QueueConfig("orders-queue", ExchangeName: "orders", RoutingKey: "orders.*")])
    .WithDataVolume("rabbit-data");

// --- Redis ---
var redis = builder.AddRedis("redis")
    .WithPassword("minha-senha")
    .WithCommander()
    .WithDataVolume("redis-data");

// --- API referenciando todos os serviços ---
builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api")
       .WithReference(cloudStorage)
       .WaitFor(cloudStorage)
       .WithReference(mongo)
       .WaitFor(mongo)
       .WithReference(gcpPubSub)
       .WaitFor(gcpPubSub)
       .WithReference(gotenberg)
       .WaitFor(gotenberg)
       .WithReference(rabbitMQ)
       .WaitFor(rabbitMQ)
       .WithReference(redis)
       .WaitFor(redis);

await builder.Build().RunAsync();
```

---

## Estrutura dos Projetos

```
src/
  MVFC.Aspire.Helpers.CloudStorage/
  MVFC.Aspire.Helpers.GcpPubSub/
  MVFC.Aspire.Helpers.Gotenberg/
  MVFC.Aspire.Helpers.Mailpit/
  MVFC.Aspire.Helpers.Mongo/
  MVFC.Aspire.Helpers.RabbitMQ/
  MVFC.Aspire.Helpers.Redis/
  MVFC.Aspire.Helpers.WireMock/
test/
  MVFC.Aspire.Helpers.Tests/
playground/
  MVFC.Aspire.Helpers.Playground.Api/
```

---

## Requisitos

- .NET 9+
- Aspire.Hosting >= 9.5.0
- Docker em execução

---

## Licença

[Apache-2.0](LICENSE)
