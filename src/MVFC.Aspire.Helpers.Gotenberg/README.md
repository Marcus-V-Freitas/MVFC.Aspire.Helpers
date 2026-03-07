# MVFC.Aspire.Helpers.Gotenberg

Helpers para integração com Gotenberg (API de conversão de documentos e PDFs) em projetos .NET Aspire.

## Visão Geral

Este projeto permite adicionar e integrar o Gotenberg como um recurso gerenciado em aplicações distribuídas .NET Aspire. Ele simplifica o provisionamento do container Gotenberg e fornece métodos de extensão para sua configuração no plano do AppHost.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.Gotenberg`](MVFC.Aspire.Helpers.Gotenberg.csproj): Biblioteca de helpers e extensões para Gotenberg.

## Funcionalidades

- Adiciona o container Gotenberg à aplicação Aspire.
- Gerencia o health check automático em `/health`.
- Injeção automática da base URL do serviço no projeto consumidor.
- Métodos de extensão para configuração rápida de portas personalizadas.

## Imagens compatíveis

- `gotenberg/gotenberg` (tag padrão `8`)

## Instalação

Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers.Gotenberg
```

## Exemplo de Uso no AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Adiciona o container do Gotenberg na porta host 3000
var gotenberg = builder.AddGotenberg("gotenberg", port: 3000);

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithReference(gotenberg)
       .WaitFor(gotenberg);

await builder.Build().RunAsync();
```

## Referência no Projeto Backend (Api, Web, etc)

Ao utilizar o `.WithReference(gotenberg)`, o AppHost injetará automaticamente uma variável de ambiente contendo o endereço acessível do Gotenberg para que a aplicação possa consumi-lo:

`GOTENBERG__BASE_URL` = `http://localhost:<porta>`

## Métodos Fluentes

| Método | Descrição |
|---|---|
| `WithDockerImage(image, tag)` | Substitui a imagem Docker utilizada ou sua tag. |

## Parâmetros de `AddGotenberg`

| Parâmetro | Tipo | Padrão | Descrição |
|---|---|---|---|
| `name` | `string` | — | Nome do recurso no Aspire. |
| `port` | `int` | `3000` | Porta HTTP exposta no host para comunicação. |

## Detalhes de Porta e Visualização

- A porta padrão mapeada é a `3000`. Internamente, o container também continua executando na `3000`.
- É registrado automaticamente um health check HTTP apontando para `http://localhost:<porta>/health`.

## Requisitos
- .NET 9+
- Aspire.Hosting >= 9.5.0

## Licença
Apache-2.0
