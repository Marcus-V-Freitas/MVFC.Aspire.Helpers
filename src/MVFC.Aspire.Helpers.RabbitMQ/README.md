# MVFC.Aspire.Helpers.RabbitMQ

Helper para integração com RabbitMQ em projetos .NET Aspire, incluindo criação automática de exchanges, queues e dead letter queues.

## Visão Geral

O MVFC.Aspire.Helpers.RabbitMQ é uma biblioteca de extensão para o .NET Aspire que facilita a integração com o RabbitMQ. Ele fornece métodos de extensão para adicionar recursos RabbitMQ ao seu aplicativo Aspire, configurar exchanges, queues e dead letter queues, e configurar referências entre projetos.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.RabbitMQ`](MVFC.Aspire.Helpers.RabbitMQ.csproj): Biblioteca de helpers e extensões para RabbitMQ.

## Funcionalidades

- Adiciona um container RabbitMQ configurado.
- Suporta Redis Commander UI.
- Suporta persistência de dados.
- Suporta senha.

## Imagens compatíveis:
 - `rabbitmq`

## Instalação

```bash
dotnet add package MVFC.Aspire.Helpers.RabbitMQ
```

## Exemplo de Uso no AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var rabbitConfig = new RabbitMQConfig(
    Exchanges: [
        new ExchangeConfig("test-exchange", "topic"), 
        new ExchangeConfig("dead-letter", "fanout")], 
    Queues: [
        new QueueConfig(Name: "test-queue", ExchangeName: "test-exchange", RoutingKey: "test.*", DeadLetterExchange: "dead-letter"), 
        new QueueConfig(Name: "empty-queue", ExchangeName: "test-exchange", RoutingKey: "empty.*"), 
        new QueueConfig(Name: "dlq", ExchangeName: "dead-letter")],
    VolumeName: "rabbit-mq");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithRabbitMQ(builder, name: "rabbitmq", rabbitMQConfig: rabbitConfig)

await builder.Build().RunAsync();
```

## Principais parâmetros

- `Port`: Porta do RabbitMQ (padrão: null para porta aleatória)
- `Password`: Senha para autenticação no RabbitMQ
- `WithManagementUI`: Booleano para habilitar o RabbitMQ Management UI.
- `ManagementUIPort`: Porta do RabbitMQ Management UI (padrão: null para porta aleatória)
- `VolumeName`: String para nome do volume de persistência.
- `Exchanges`: Lista de exchanges a serem criados
- `Queues`: Lista de queues a serem criados

## Outros parâmetros Opcionais importantes:

- **connectionStringSection** (Opcional): Define o caminho da variável de ambiente ou configuração que contém a string de conexão do RabbitMQ. O padrão é "ConnectionStrings:rabbitmq". Cada `:` indica um nível/seção dentro do arquivo `appsettings.json`, permitindo acessar configurações aninhadas, por exemplo:

```json
{
  "ConnectionStrings": {
    "rabbitmq": "localhost:5672"
  }
}
```

## Métodos Públicos

- `WithRabbitMQ`: Adiciona um recurso RabbitMQ ao projeto Aspire.
- `AddRabbitMQ`: Adiciona um recurso RabbitMQ ao projeto Aspire.
- `WaitForRabbitMQ`: Aguarda até que o recurso RabbitMQ esteja disponível antes de iniciar o projeto Aspire.

## Requisitos
- .NET 9+
- Aspire.Hosting >= 9.5.0

## Licença
Apache-2.0      