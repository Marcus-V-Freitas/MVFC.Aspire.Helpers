# MVFC.Aspire.Helpers

Conjunto de helpers para projetos .NET Aspire, incluindo integrações com Google Pub/Sub, Cloud Storage (emulador GCS) e MongoDB (com Replica Sets).

## Visão Geral

Este projeto facilita a configuração e integração de recursos essenciais para aplicações distribuídas .NET Aspire, fornecendo métodos de extensão para:

- **Google Cloud Storage (emulador GCS)**
- **MongoDB com Replica Set**
- **Google Pub/Sub (emulador e UI)**

Além disso, inclui uma API de exemplo ([MVFC.Aspire.Helpers.Api](MVFC.Aspire.Helpers.Api/MVFC.Aspire.Helpers.Api.csproj)) e um AppHost para orquestração ([MVFC.Aspire.Helpers.AppHost.AppHost](MVFC.Aspire.Helpers.AppHost/MVFC.Aspire.Helpers.AppHost.AppHost/MVFC.Aspire.Helpers.AppHost.AppHost.csproj)).

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers`](MVFC.Aspire.Helpers/MVFC.Aspire.Helpers.csproj): Biblioteca de helpers e extensões.
- [`MVFC.Aspire.Helpers.Api`](MVFC.Aspire.Helpers.Api/MVFC.Aspire.Helpers.Api.csproj): API de exemplo com endpoints para MongoDB, Cloud Storage e Pub/Sub.
- [`MVFC.Aspire.Helpers.AppHost.AppHost`](MVFC.Aspire.Helpers.AppHost/MVFC.Aspire.Helpers.AppHost.AppHost/MVFC.Aspire.Helpers.AppHost.AppHost.csproj): Orquestração de teste dos recursos usando Aspire.
- [`MVFC.Aspire.Helpers.AppHost.Tests`](MVFC.Aspire.Helpers.AppHost/MVFC.Aspire.Helpers.AppHost.Tests/MVFC.Aspire.Helpers.AppHost.Tests.csproj): Testes automatizados para os endpoints.

## Funcionalidades

### Cloud Storage

- Adiciona e integra um emulador GCS usando a imagem `fsouza/fake-gcs-server`.
- Permite persistência opcional dos buckets via bind mount.
- Exemplo de uso: [`CloudStorageExtensions`](MVFC.Aspire.Helpers/CloudStorage/CloudStorageExtensions.cs).

### MongoDB

- Adiciona um container MongoDB configurado como Replica Set.
- Inicializa automaticamente o Replica Set via script.
- Exemplo de uso: [`MongoExtensions`](MVFC.Aspire.Helpers/Mongo/MongoExtensions.cs).

### Pub/Sub

- Adiciona o emulador do Google Pub/Sub e interface de administração (UI).
- Cria tópicos e assinaturas automaticamente conforme configuração.
- Suporte a assinaturas do tipo push e pull.
- Exemplo de uso: [`PubSubExtensions`](MVFC.Aspire.Helpers/PubSub/PubSubExtensions.cs).

### API de Exemplo

- Endpoints para testar integração com MongoDB, Cloud Storage e Pub/Sub:
  - `/api/mongo`
  - `/api/bucket/{bucketName}`
  - `/api/pub-sub-enter`
  - `/api/pub-sub-exit`
- Implementações em [`Endpoints`](MVFC.Aspire.Helpers.Api/Endpoints/DefaultEndpoints.cs).

## Instalação

Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers
```

## Exemplo de Uso no AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var messageConfig = new MessageConfig(
                            TopicName: "test-topic",
                            SubscriptionName: "test-subscription",
                            PushEndpoint: "/api/pub-sub-exit");

var pubSubConfig = new PubSubConfig(
                            projectId: "test-project",
                            messageConfig: messageConfig);

builder.AddProject<Projects.MVFC_Aspire_Helpers_Api>("api-exemplo")
       .WithCloudStorage(builder, "cloud-storage", "./bucket-data")
       .WithMongoReplicaSet(builder, "mongo")
       .WithGcpPubSub(builder, "gcp-pubsub", pubSubConfig);

await builder.Build().RunAsync();
```