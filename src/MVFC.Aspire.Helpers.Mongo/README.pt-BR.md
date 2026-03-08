# MVFC.Aspire.Helpers.Mongo

> 🇺🇸 [Read in English](README.md)

Helpers para integração com MongoDB em projetos .NET Aspire, incluindo suporte a Replica Set e inicialização automática.

## Visão Geral

Este projeto facilita a configuração e integração do MongoDB em aplicações distribuídas .NET Aspire, fornecendo métodos de extensão para:

- Adicionar um container MongoDB configurado como Replica Set.
- Inicializar automaticamente o Replica Set via script.
- Popular o banco com dados de exemplo usando dumps customizados.

## Por que usar Replica Set?

O MongoDB só permite o uso de transações multi-documento quando está configurado como Replica Set, mesmo em ambientes locais.  
Ao utilizar o helper com Replica Set, você pode:

- **Simular transações locais:**  
  Permite testar operações de transação (commit/rollback) em múltiplos documentos e coleções, igual ao ambiente de produção.
- **Alta disponibilidade e tolerância a falhas:**  
  Replica Set é a base para recursos avançados do MongoDB, como failover e redundância (mesmo que localmente, já prepara o ambiente).

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.Mongo`](MVFC.Aspire.Helpers.Mongo.csproj): Biblioteca de helpers e extensões para MongoDB.

## Funcionalidades

- Adiciona um container MongoDB com Replica Set.
- Inicialização automática do Replica Set.
- Suporte para popular coleções com dados de exemplo.
- Métodos de extensão para facilitar a configuração no AppHost.

## Imagens compatíveis:
 - `mongo`

## Instalação

Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers.Mongo
```

## Exemplo de Uso no AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

IReadOnlyCollection<IMongoClassDump> dumps = [
    new MongoClassDump<TestDatabase>(
        DatabaseName: "TestDatabase",
        CollectionName: "TestCollection",
        Quantity: 100,
        Faker: new Faker<TestDatabase>()
              .CustomInstantiator(f => new TestDatabase(f.Person.FirstName, f.Person.Cpf())))
];

var mongo = builder.AddMongoReplicaSet("mongo")
    .WithDumps(dumps)
    .WithDataVolume("mongo-data");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithReference(mongo)
       .WaitFor(mongo);

await builder.Build().RunAsync();
```

## Métodos Fluentes

| Método | Descrição |
|---|---|
| `WithDockerImage(image, tag)` | Substitui a imagem Docker utilizada. |
| `WithDumps(dumps)` | Configura dumps de dados a executar na inicialização. |
| `WithDataVolume(volumeName)` | Habilita persistência com volume Docker. |

## Popular dados de exemplo

O `MongoClassDump<T>` é uma classe utilizada para facilitar a inserção automática de dados de exemplo em coleções do MongoDB durante a inicialização do ambiente. Ela serve como um "template" para popular o banco com documentos fictícios, útil para testes e desenvolvimento local.

**Parâmetros principais:**
- `DatabaseName`: Nome do banco de dados.
- `CollectionName`: Nome da coleção.
- `Quantity`: Quantidade de documentos.
- `Faker`: Gerador de dados (ex: usando a biblioteca **Bogus** com a classe **Faker**).

## Outros parâmetros Opcionais importantes:

- **volumeName** (Opcional): Representa o nome do volume docker local para persistir dados entre as depurações. O default é nulo (volume descartado entre execuções).
- **connectionStringSection** (Opcional): Define o caminho da variável de ambiente ou configuração que contém a string de conexão do MongoDB. O padrão é `"ConnectionStrings:mongo"`. Cada `:` indica um nível/seção dentro do arquivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "mongo": "mongodb://localhost:27017/"
  }
}
```

## Detalhes de Visualização e Porta do MongoDB

- **Porta utilizada:** `27017` (padrão do MongoDB).
- **Visualizar bancos de dados:**  
  Conecte-se via cliente MongoDB (ex: MongoDB Compass, Robo 3T, mongosh) usando:  
  `mongodb://localhost:27017/`

## Requisitos
- .NET 9+
- Aspire.Hosting >= 9.5.0
- Bogus >= 35.6.0
- MongoDB.Driver >= 3.5.0

## Licença
Apache-2.0
