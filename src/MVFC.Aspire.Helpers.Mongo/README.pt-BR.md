# MVFC.Aspire.Helpers.Mongo

> 🇺🇸 [Read in English](README.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.Mongo)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Mongo)


Helpers para integração com MongoDB em projetos .NET Aspire, incluindo suporte a Replica Set e inicialização automática.

## Motivação

Subir o MongoDB localmente é fácil; subir um **Replica Set com comportamento realista e dados de seed** não é tão trivial.

Pontos de atrito comuns no desenvolvimento local:

- Criar e inicializar o Replica Set via scripts customizados.
- Lembrar quais portas estão expostas e como montar a connection string.
- Carregar dados de seed toda vez que o app sobe, sem ficar chamando `mongorestore` na mão.

Com o .NET Aspire você pode modelar o container do MongoDB, mas ainda precisa ligar:

- Argumentos de Replica Set e scripts de init.
- Volume de dados para persistência entre execuções.
- Configuração da connection string para os projetos.
- Qualquer lógica de seed que precisa rodar uma vez.

O `MVFC.Aspire.Helpers.Mongo` empacota essas preocupações em uma API focada:

- `AddMongoReplicaSet(...)` para ter um Replica Set pronto para uso.
- Métodos fluentes como `WithDumps(...)` e `WithDataVolume(...)`.
- `project.WithReference(mongo)` para injetar a connection string e disparar dumps automaticamente quando o recurso estiver pronto.

## Visão Geral

Este projeto facilita a configuração e integração do MongoDB em aplicações distribuídas .NET Aspire, fornecendo métodos de extensão para:

- Adicionar um container MongoDB configurado como Replica Set.
- Inicializar automaticamente o Replica Set via script.
- Popular o banco com dados de exemplo usando dumps customizados.

### Por que usar Replica Set?

O MongoDB só permite transações multi-documento quando configurado como Replica Set, mesmo em ambientes locais.
Ao utilizar o helper com Replica Set, você pode:

- **Simular transações locais:**  
  Testar operações de transação (commit/rollback) em múltiplos documentos e coleções, igual ao ambiente de produção.
- **Preparar para alta disponibilidade:**  
  Replica Set é a base para recursos avançados do MongoDB como failover e redundância; mesmo localmente, ajuda a rodar mais próximo de um ambiente real.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.Mongo`](MVFC.Aspire.Helpers.Mongo.csproj): Biblioteca de helpers e extensões para MongoDB.

## Funcionalidades

- Adiciona um container MongoDB com Replica Set.
- Inicialização automática do Replica Set.
- Suporte para popular coleções com dados de exemplo.
- Métodos de extensão para facilitar a configuração no AppHost.

### Imagens compatíveis

- `mongo`

## Instalação

Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers.Mongo
```

## Uso rápido no Aspire (AppHost)

Exemplo mínimo usando Bogus para popular dados de teste:

```csharp
using Aspire.Hosting;
using Bogus;
using MVFC.Aspire.Helpers.Mongo;

var builder = DistributedApplication.CreateBuilder(args);

IReadOnlyCollection<IMongoClassDump> dumps =
[
    new MongoClassDump<TestDatabase>(
        DatabaseName: "TestDatabase",
        CollectionName: "TestCollection",
        Quantity: 100,
        Faker: new Faker<TestDatabase>()
            .CustomInstantiator(f => new TestDatabase(
                f.Person.FirstName,
                f.Person.Cpf())))
];

var mongo = builder.AddMongoReplicaSet("mongo")
    .WithDumps(dumps)
    .WithDataVolume("mongo-data");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithReference(mongo)
       .WaitFor(mongo);

await builder.Build().RunAsync();
```

O que essa configuração entrega:

- Um Replica Set MongoDB rodando em Docker.
- Um volume Docker persistente `mongo-data` (se configurado).
- Banco/coleção populados automaticamente com dados fictícios na inicialização.
- Connection string injetada na configuração do projeto.

## Métodos Fluentes

| Método                            | Descrição                                              |
|-----------------------------------|--------------------------------------------------------|
| `WithDockerImage(image, tag)`     | Substitui a imagem Docker utilizada.                   |
| `WithDumps(dumps)`                | Configura dumps de dados a executar na inicialização.  |
| `WithDataVolume(volumeName)`      | Habilita persistência com volume Docker.               |

## Popular dados de exemplo

O `MongoClassDump<T>` é uma classe utilizada para facilitar a inserção automática de dados de exemplo em coleções do MongoDB durante a inicialização do ambiente. Ela serve como um **template** para popular o banco com documentos fictícios, útil para testes e desenvolvimento local.

**Parâmetros principais:**

- `DatabaseName`: Nome do banco de dados.
- `CollectionName`: Nome da coleção.
- `Quantity`: Quantidade de documentos.
- `Faker`: Gerador de dados (ex: usando a biblioteca **Bogus** com a classe `Faker`).

## Parâmetros opcionais

- **`volumeName`** (opcional):  
  Nome do volume Docker local para persistir dados entre as sessões de depuração.  
  Padrão: `null` (volume descartado entre execuções).

- **`connectionStringSection`** (opcional):  
  Caminho da configuração que contém a connection string do MongoDB.  
  Padrão: `"ConnectionStrings:mongo"`.

```json
{
  "ConnectionStrings": {
    "mongo": "mongodb://localhost:27017/"
  }
}
```

## Visualização e porta

- **Porta utilizada:** `27017` (padrão do MongoDB).
- **Visualizar bancos de dados:**  
  Conecte-se via cliente MongoDB (MongoDB Compass, Robo 3T, `mongosh`) usando:

  `mongodb://localhost:27017/`

## Requisitos

- .NET 9+
- Aspire.Hosting >= 9.5.0
- Bogus >= 35.6.0
- MongoDB.Driver >= 3.5.0

## Licença

Apache-2.0
