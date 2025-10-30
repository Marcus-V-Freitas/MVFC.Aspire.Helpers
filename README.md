# MVFC.Aspire.Helpers

Este projeto facilita a configuração e integração de recursos essenciais para aplicações distribuídas .NET Aspire, fornecendo métodos de extensão para:

### [`Cloud Storage`](./src/MVFC.Aspire.Helpers.CloudStorage/README.md)

- Adiciona e integra um emulador GCS.
- Permite persistência opcional dos buckets via bind mount.
- Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers.CloudStorage
```

---

### [`Mongo`](./src/MVFC.Aspire.Helpers.Mongo/README.md)

- Adiciona um container MongoDB configurado como Replica Set.
- Inicializa automaticamente o Replica Set via script.
- Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers.Mongo
```

---

### [`GCP Pub/Sub`](./src/MVFC.Aspire.Helpers.GcpPubSub/README.md)

- Adiciona o emulador do Google Pub/Sub e UI.
- Cria tópicos e assinaturas automaticamente conforme configuração.
- Suporte a assinaturas do tipo push e pull.
- Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers.GcpPubSub
```

---

## API de Exemplo (Playground)

- Endpoints para testar integração com MongoDB, Cloud Storage e Pub/Sub:
  - `/api/mongo`
  - `/api/bucket/{bucketName}`
  - `/api/pub-sub-enter`
  - `/api/pub-sub-exit`
- Implementações no projeto [`MVFC.Aspire.Helpers.Playground.Api`](./playground/MVFC.Aspire.Helpers.Playground.Api/).

## Integração no Aspire no [`MVFC.Aspire.Helpers.Playground.AppHost`](./playground/MVFC.Aspire.Helpers.Playground.AppHost/AppHost.cs)

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

IList<IMongoClassDump> dumps = [
    new MongoClassDump<TestDatabase>(DatabaseName: "TestDatabase", CollectionName: "TestCollection", Quantity: 100,
        Faker: new Faker<TestDatabase>()
              .CustomInstantiator(f => new TestDatabase(f.Person.FirstName, f.Person.Cpf())))
];

builder.AddProject<Projects.MVFC_Aspire_Helpers_Api>("api-exemplo")
       .WithCloudStorage(builder, "cloud-storage", localBucketFolder: "./bucket-data")
       .WithMongoReplicaSet(builder, "mongo", dumps: dumps)
       .WithGcpPubSub(builder, "gcp-pubsub", pubSubConfig);

await builder.Build().RunAsync();
```