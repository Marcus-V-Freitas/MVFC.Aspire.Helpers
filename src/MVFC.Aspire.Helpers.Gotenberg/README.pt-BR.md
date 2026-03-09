# MVFC.Aspire.Helpers.Gotenberg

> 🇺🇸 [Read in English](README.md)

Helpers para integração com Gotenberg (API de conversão de documentos e PDFs) em projetos .NET Aspire.

## Motivação

Rodar o Gotenberg localmente normalmente envolve:

- Fazer pull da imagem Docker e lembrar a tag e as portas corretas.
- Expor manualmente o endpoint HTTP para as aplicações.
- Deixar a base URL do serviço de PDF hard-code em código ou configuração.

Com o .NET Aspire você pode modelar o container, mas ainda precisa:

- Configurar imagem, portas e health checks.
- Decidir como expor a base URL para cada projeto.
- Manter host/porta em sincronia entre o AppHost e os serviços.

O `MVFC.Aspire.Helpers.Gotenberg` ataca exatamente esse problema:

- `AddGotenberg(...)` sobe a imagem oficial `gotenberg/gotenberg:8` com endpoint HTTP e health check configurados.
- `project.WithReference(gotenberg)` injeta `GOTENBERG__BASE_URL` para que a API/worker leia apenas da configuração.

## Visão Geral

Este projeto permite adicionar e integrar o Gotenberg como um recurso gerenciado em aplicações distribuídas .NET Aspire. Ele simplifica o provisionamento do container Gotenberg e fornece métodos de extensão para sua configuração no AppHost.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.Gotenberg`](MVFC.Aspire.Helpers.Gotenberg.csproj): Biblioteca de helpers e extensões para Gotenberg.

## Funcionalidades

- Adiciona o container Gotenberg à aplicação Aspire.
- Gerencia o health check automático em `/health`.
- Injeção automática da base URL do serviço no projeto consumidor.
- Métodos de extensão para configuração rápida de portas personalizadas.

### Imagens compatíveis

- `gotenberg/gotenberg` (tag padrão `8`)

## Instalação

Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers.Gotenberg
```

## Uso rápido no Aspire (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.Gotenberg;

var builder = DistributedApplication.CreateBuilder(args);

// Adiciona o container do Gotenberg na porta host 3000
var gotenberg = builder.AddGotenberg("gotenberg", port: 3000);

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithReference(gotenberg)
       .WaitFor(gotenberg);

await builder.Build().RunAsync();
```

## Referência no projeto backend (API, Web, etc.)

Ao utilizar `.WithReference(gotenberg)`, o AppHost injetará automaticamente uma variável de ambiente contendo o endereço acessível do Gotenberg:

- `GOTENBERG__BASE_URL` = `http://localhost:<porta>`

Use esse valor para configurar o cliente HTTP que aponta para o Gotenberg.

## Métodos Fluentes

| Método                        | Descrição                                         |
|------------------------------|---------------------------------------------------|
| `WithDockerImage(image, tag)` | Substitui a imagem Docker utilizada ou sua tag.   |

## Parâmetros de `AddGotenberg`

| Parâmetro | Tipo   | Padrão | Descrição                                      |
|----------|--------|--------|------------------------------------------------|
| `name`   | string | —      | Nome do recurso no Aspire.                     |
| `port`   | int    | `3000` | Porta HTTP exposta no host para comunicação.   |

## Detalhes de porta e visualização

- Porta padrão mapeada: `3000` (internamente também `3000`).
- Health check: `http://localhost:<porta>/health`.

## Requisitos

- .NET 9+
- Aspire.Hosting >= 9.5.0

## Licença

Apache-2.0
