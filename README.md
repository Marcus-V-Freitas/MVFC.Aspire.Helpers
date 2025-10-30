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

### [`WireMock`](./src/MVFC.Aspire.Helpers.WireMock/README.md)

- Adiciona e integra um servidor WireMock.Net embutido para simular APIs HTTP.
- Permite definir endpoints mockados, métodos, autenticação, headers e respostas customizadas.
- Gerencia ciclo de vida do recurso WireMock no ambiente Aspire.
- Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers.WireMock
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
    new MongoClassDump<TestDatabase>("TestDatabase", "TestCollection", 100,
        new Faker<TestDatabase>()
              .CustomInstantiator(f => new TestDatabase(f.Person.FirstName, f.Person.Cpf())))
];

var wireMock = builder.AddWireMock("wireMock", port: 8080, configure: static (server) => {
    server.Endpoint("/api/echo")
          .WithDefaultBodyType(BodyType.String)
          .OnPost<string, string>(body => ($"Echo: {body}", HttpStatusCode.Created, null));

    server.Endpoint("/api/test")
          .WithDefaultBodyType(BodyType.String)
          .OnGet<string>(() => ("Aspire GET OK", HttpStatusCode.OK, null));

    server.Endpoint("/api/secure")
           .RequireBearer("mytoken", "Unauthorized", BodyType.String)
           .OnGet(() => ("Secret Data", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/secure")
          .RequireBearer("mytoken", "Unauthorized", BodyType.String)
          .OnGet(() => ("Secret Data", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/put")
           .WithDefaultBodyType(BodyType.String)
           .OnPut<string, string>(req => ($"Echo: {req}", HttpStatusCode.Accepted, BodyType.String));

    server.Endpoint("/api/customauth")
        .WithDefaultErrorStatusCode(HttpStatusCode.Forbidden)
        .RequireCustomAuth(req => (req.Headers!.ContainsKey("X-Test"), "Forbidden", BodyType.String))
        .OnGet(() => ("Authorized", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/headers")
        .WithResponseHeaders(new() { { "X-Test", ["v1", "v2"] } })
        .WithResponseHeader("X-Other", "v3")
        .OnGet(() => ("Headers OK", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/error")
        .WithRequestBodyType(BodyType.String)
        .WithDefaultErrorStatusCode((HttpStatusCode)418)
        .OnGet(() => ("I am a teapot", (HttpStatusCode)418, BodyType.String));

    server.Endpoint("/api/delete")
       .WithResponseBodyType(BodyType.String)
       .WithResponseHeader("v1", "v1")
       .WithResponseHeaders(new() { { "v1", ["v2", "v3"] } })
       .WithResponseHeader("v1", "v4")
       .OnDelete<string>(() => (null!, HttpStatusCode.NoContent, null));

    server.Endpoint("/api/form")
        .WithDefaultBodyType(BodyType.FormUrlEncoded)
        .OnPost<Dictionary<string, string>, IDictionary<string, string>>(body => (body, HttpStatusCode.OK, BodyType.FormUrlEncoded));

    server.Endpoint("/api/form-wrong")
       .WithDefaultBodyType(BodyType.FormUrlEncoded)
       .OnPost<string, string>(body => (body, HttpStatusCode.OK, BodyType.FormUrlEncoded));

    server.Endpoint("/api/patch")
        .WithDefaultBodyType(BodyType.String)
        .OnPatch<string, string>(body => ($"Patched: {body}", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/bytes")
        .WithDefaultBodyType(BodyType.Bytes)
        .OnPost<byte[], byte[]>(body => (body, HttpStatusCode.OK, BodyType.Bytes));

    server.Endpoint("/api/unsupported")
        .WithDefaultBodyType((BodyType)999) // BodyType não suportado
        .OnPost<string, string>(_ => ("Not Supported", HttpStatusCode.NotImplemented, null));

    server.Endpoint("/api/json")
        .WithDefaultBodyType(BodyType.Json)
        .OnPost<JsonModel, JsonModel>(body => (body, HttpStatusCode.OK, BodyType.Json));
});

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithCloudStorage(builder, name: "cloud-storage", localBucketFolder: "./bucket-data")
       .WithMongoReplicaSet(builder, name: "mongo", dumps: dumps)
       .WithGcpPubSub(builder, name: "gcp-pubsub", pubSubConfig: pubSubConfig);

await builder.Build().RunAsync();
```