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
| [MVFC.Aspire.Helpers.Keycloak](src/MVFC.Aspire.Helpers.Keycloak/README.md) | Keycloak | `MVFC.Aspire.Helpers.Keycloak` |

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
dotnet add package MVFC.Aspire.Helpers.Keycloak
```

---

## Exemplo de Uso no AppHost

O exemplo abaixo demonstra como configurar todos os helpers em um único `AppHost`:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var messageConfig = new MessageConfig(
                            TopicName: "test-topic",
                            SubscriptionName: "test-subscription",
                            PushEndpoint: "/api/pub-sub-exit") {
    DeadLetterTopic = "test-dead-letter-topic",
    MaxDeliveryAttempts = 5,
    AckDeadlineSeconds = 300,
};

var pubSubConfig = new PubSubConfig(
                            projectId: "test-project",
                            messageConfig: messageConfig);

IReadOnlyCollection<IMongoClassDump> dumps = [
    new MongoClassDump<TestDatabase>("TestDatabase", "TestCollection", 100,
        new Faker<TestDatabase>()
              .CustomInstantiator(f => new TestDatabase(f.Person.FirstName, f.Person.Cpf())))
];

var keycloak = builder.AddKeycloak("keycloak")
    .WithAdminCredentials("admin", "Admin@123")
    .WithSeeds([new MyAppRealm()])
    .WithImportStrategy(KeycloakImportStrategy.IgnoreExisting)
    .WithDataVolume("key-cloak-data");

// Criar recursos com padrão builder
var cloudStorage = builder.AddCloudStorage("cloud-storage")
    .WithBucketFolder("./bucket-data");

var mongo = builder.AddMongoReplicaSet("mongo")
    .WithDumps(dumps);

var pubSub = builder.AddGcpPubSub("gcp-pubsub")
    .WithPubSubConfigs(pubSubConfig);

var pubSubUI = builder.AddGcpPubSubUI("gcp-pubsub-ui")
    .WithReference(pubSub);

var mailpit = builder.AddMailpit("mailpit");

var rabbitMQ = builder.AddRabbitMQ("rabbitmq")
    .WithExchanges([
        new ExchangeConfig("test-exchange", "topic"),
        new ExchangeConfig("dead-letter", "fanout")])
    .WithQueues([
        new QueueConfig(Name: "test-queue", ExchangeName: "test-exchange", RoutingKey: "test.*", DeadLetterExchange: "dead-letter"),
        new QueueConfig(Name: "empty-queue", ExchangeName: "test-exchange", RoutingKey: "empty.*"),
        new QueueConfig(Name: "dlq", ExchangeName: "dead-letter")])
    .WithDataVolume("rabbit-mq");

var redis = builder.AddRedis("redis")
    .WithCommander()
    .WithDataVolume("redis-data");

// --- Gotenberg ---
var gotenberg = builder.AddGotenberg("gotenberg", port: 3000);

// Referenciar recursos no projeto
var api = builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
                 .WithReference(cloudStorage)
                 .WaitFor(cloudStorage)
                 .WithReference(mongo)
                 .WaitFor(mongo)
                 .WithReference(pubSub)
                 .WaitFor(pubSub)
                 .WithReference(mailpit)
                 .WaitFor(mailpit)
                 .WithReference(rabbitMQ)
                 .WaitFor(rabbitMQ)
                 .WithReference(redis)
                 .WaitFor(redis)
                 .WithReference(pubSubUI)
                 .WaitFor(pubSubUI)
                 .WithReference(gotenberg)
                 .WaitFor(gotenberg)
                 .WaitFor(keycloak)
                 .WithReference(keycloak,
                         realmName: "my-app",
                         clientId: "my-api",
                         clientSecret: "api-secret-1234");

var wireMock = builder.AddWireMock("wireMock", port: 7070, configure: (server) => {
    /* Exemplo reduzido para o README. Acesse Playground.AppHost/AppHost.cs para mais detalhes */
    server.Endpoint("/api/test")
          .WithDefaultBodyType(BodyType.String)
          .OnGet<string>(() => ("Aspire GET OK", HttpStatusCode.OK, null));
});

await builder.Build().RunAsync().ConfigureAwait(false);
```

---

## Estrutura dos Projetos

```
src/
  MVFC.Aspire.Helpers.CloudStorage/
  MVFC.Aspire.Helpers.GcpPubSub/
  MVFC.Aspire.Helpers.Gotenberg/
  MVFC.Aspire.Helpers.Keycloak/
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
