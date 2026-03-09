# MVFC.Aspire.Helpers.RabbitMQ

> 🇺🇸 [Read in English](README.md)

Helper para integração com RabbitMQ em projetos .NET Aspire, incluindo criação automática de exchanges, queues e dead letter queues.

## Motivação

Usar RabbitMQ em ambientes locais normalmente requer:

- Manter um `definitions.json` com exchanges/queues/bindings.
- Carregar esse arquivo manualmente ou rodar comandos `rabbitmqctl`.
- Repetir a configuração para cada solução Aspire.

Com o .NET Aspire você pode subir o container do RabbitMQ, mas ainda precisa:

- Configurar usuários/senhas e a interface de gerenciamento.
- Declarar exchanges, queues e DLQs de forma consistente.
- Manter os detalhes de conexão sincronizados com as aplicações.

O `MVFC.Aspire.Helpers.RabbitMQ` empacota tudo em uma API fluente:

- `AddRabbitMQ(...)` para provisionar o RabbitMQ no Aspire.
- `WithExchanges(...)` e `WithQueues(...)` para declarar a topologia a partir do código.
- `WithDataVolume(...)` e `WithCredentials(...)` para persistência e segurança.
- `project.WithReference(rabbitMQ)` para ligar os serviços ao broker.

## Visão Geral

O MVFC.Aspire.Helpers.RabbitMQ é uma biblioteca de extensão para o .NET Aspire que facilita a integração com o RabbitMQ. Ele fornece métodos de extensão para adicionar recursos RabbitMQ ao seu aplicativo Aspire, configurar exchanges, queues e dead letter queues, e configurar referências entre projetos.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.RabbitMQ`](MVFC.Aspire.Helpers.RabbitMQ.csproj): Biblioteca de helpers e extensões para RabbitMQ.

## Funcionalidades

- Adiciona um container RabbitMQ configurado.
- Suporte a exchanges e queues customizadas via `ExchangeConfig`/`QueueConfig`.
- Suporte a dead letter exchanges (DLX).
- Suporte a TTL de mensagens por fila.
- Suporte a persistência de dados via volume Docker.
- Suporte a credenciais customizadas.
- Suporte ao RabbitMQ Management UI.

### Imagens compatíveis

- `rabbitmq`

## Instalação

```sh
dotnet add package MVFC.Aspire.Helpers.RabbitMQ
```

## Uso rápido no Aspire (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.RabbitMQ;

var builder = DistributedApplication.CreateBuilder(args);

var rabbitMQ = builder.AddRabbitMQ("rabbitmq")
    .WithCredentials(username: "admin", password: "password")
    .WithExchanges([
        new ExchangeConfig("test-exchange", "topic"),
        new ExchangeConfig("dead-letter", "fanout")
    ])
    .WithQueues([
        new QueueConfig("test-queue", ExchangeName: "test-exchange", RoutingKey: "test.*", DeadLetterExchange: "dead-letter"),
        new QueueConfig("empty-queue", ExchangeName: "test-exchange", RoutingKey: "empty.*"),
        new QueueConfig("dlq", ExchangeName: "dead-letter")
    ])
    .WithDataVolume("rabbit-mq");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithReference(rabbitMQ)
       .WaitFor(rabbitMQ);

await builder.Build().RunAsync();
```

## Principais parâmetros do `AddRabbitMQ`

- `name`: Nome do recurso RabbitMQ.
- `amqpPort` *(opcional)*: Porta AMQP (padrão: `5672`).
- `httpPort` *(opcional)*: Porta do Management UI (padrão: `15672`).

## Métodos Fluentes

| Método                                | Descrição                                   |
|--------------------------------------|---------------------------------------------|
| `WithDockerImage(image, tag)`        | Substitui a imagem Docker utilizada.        |
| `WithCredentials(username, password)`| Define usuário e senha.                     |
| `WithExchanges(exchanges)`           | Configura exchanges a serem criadas.        |
| `WithQueues(queues)`                 | Configura queues a serem criadas.           |
| `WithDataVolume(volumeName)`         | Habilita persistência com volume Docker.    |

## Configuração de Exchanges e Queues

### `ExchangeConfig`

| Parâmetro   | Tipo   | Padrão     | Descrição                                          |
|------------|--------|------------|----------------------------------------------------|
| `Name`     | string | —          | Nome do exchange.                                  |
| `Type`     | string | `"direct"` | Tipo: `direct`, `topic`, `fanout`, `headers`.     |
| `Durable`  | bool   | `true`     | Durável entre reinicializações.                    |
| `AutoDelete` | bool | `false`    | Auto deletar quando não utilizado.                 |

### `QueueConfig`

| Parâmetro            | Tipo    | Padrão | Descrição                                        |
|---------------------|---------|--------|--------------------------------------------------|
| `Name`              | string  | —      | Nome da queue.                                   |
| `ExchangeName`      | string? | `null` | Exchange ao qual a queue será vinculada.          |
| `RoutingKey`        | string? | `null` | Routing key do binding (padrão: nome da queue).  |
| `Durable`           | bool    | `true` | Durável entre reinicializações.                  |
| `AutoDelete`        | bool    | `false`| Auto deletar quando não utilizada.               |
| `DeadLetterExchange`| string? | `null` | Exchange de dead letter.                         |
| `MessageTTL`        | int?    | `null` | TTL de mensagens em milissegundos.               |

## Parâmetros opcionais

- **`connectionStringSection`** (opcional):  
  Caminho da configuração que contém a connection string do RabbitMQ.  
  Padrão: `"ConnectionStrings:rabbitmq"`.

```json
{
  "ConnectionStrings": {
    "rabbitmq": "localhost:5672"
  }
}
```

## Requisitos

- .NET 9+
- Aspire.Hosting >= 9.5.0

## Licença

Apache-2.0
