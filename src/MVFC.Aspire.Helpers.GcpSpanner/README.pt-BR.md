# MVFC.Aspire.Helpers.GcpSpanner

> 🇺🇸 [Read in English](README.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010%20%7C%2011-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.GcpSpanner)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.GcpSpanner)

Helpers para integração com Google Cloud Spanner em projetos .NET Aspire, incluindo suporte ao emulador.

## Motivação

As configurações do Spanner podem ser adicionadas antes ou depois de `WithReference`; o provisionamento lê a configuração vigente quando o emulador fica pronto.

Trabalhar com Google Cloud Spanner localmente normalmente significa:

- Subir um container de emulador na mão.
- Lembrar portas, project IDs, instâncias, bancos de dados e variáveis de ambiente.
- Criar manualmente instâncias e bancos de dados, e executar scripts DDL.

Com o .NET Aspire você pode definir containers, mas ainda precisa:

- Configurar a imagem do emulador e suas portas.
- Manter variáveis de ambiente do emulador em sincronia entre projetos.
- Definir instâncias/databases de forma consistente antes da aplicação rodar.

O `MVFC.Aspire.Helpers.GcpSpanner` fornece:

- `AddGcpSpanner(...)` para iniciar o emulador.
- `WithSpannerConfigs(...)` para descrever instâncias, bancos de dados e executar DDLs via código.
- `WithReference(...)` para ligar projetos ao emulador e injetar configurações de conexão automaticamente.

## Visão Geral

Este projeto facilita a configuração e integração do Google Cloud Spanner em aplicações distribuídas .NET Aspire, fornecendo métodos de extensão para:

- Adicionar o emulador do Google Cloud Spanner.
- Configurar instâncias e bancos de dados automaticamente na inicialização.
- Executar instruções DDL logo após a criação do banco de dados.
- Injetar adequadamente a connection string do host do emulador para detecção automática pelos clientes do Spanner.

## Vantagens do Emulador Spanner

- Simula bancos de dados Spanner localmente para desenvolvimento e testes.
- Permite testar mudanças de esquema e execução de consultas sem depender da infraestrutura do Google Cloud.
- Facilita o desenvolvimento de implementações robustas de armazenamento de dados.

## Imagens compatíveis

- **Emulator**:
  - `gcr.io/cloud-spanner-emulator/emulator` (Padrão no helper do Aspire)

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.GcpSpanner`](MVFC.Aspire.Helpers.GcpSpanner.csproj): Biblioteca de helpers e extensões para Spanner.

## Funcionalidades

- Adiciona o emulador do Google Cloud Spanner.
- Cria instâncias e bancos de dados conforme configuração.
- Executa instruções DDL personalizadas durante o provisionamento.
- Uma sondagem TCP verifica a conectividade na porta gRPC. O provisionamento é separado; uma porta aberta não comprova a criação de instâncias, bancos ou DDL.
- Métodos de extensão para facilitar a configuração no AppHost.

## Instalação

```sh
dotnet add package MVFC.Aspire.Helpers.GcpSpanner
dotnet add package Aspire.Hosting.Dotnet --version 17.0.0
```

Os comandos e exemplos com `AddDotnetProject` usam um AppHost .NET 10/11 com Aspire 17. Em AppHosts .NET 9, mantenha Aspire 13.6.5, omita `Aspire.Hosting.Dotnet` 17 e use `AddProject<Projects.MyApi>("api")`. Ajuste o caminho do projeto ao seu AppHost.

## Uso rápido no Aspire (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.GcpSpanner;
using MVFC.Aspire.Helpers.GcpSpanner.Models;

var builder = DistributedApplication.CreateBuilder(args);

var spannerConfig = new SpannerConfig(
    ProjectId: "test-project",
    InstanceId: "dev-instance",
    DatabaseId: "dev-db",
    DdlStatements:
    [
        """
        CREATE TABLE Users (
            UserId STRING(36) NOT NULL,
            Name STRING(256) NOT NULL,
            CreatedAt TIMESTAMP NOT NULL OPTIONS (allow_commit_timestamp=true)
        ) PRIMARY KEY (UserId)
        """
    ]);

var spanner = builder.AddGcpSpanner("gcp-spanner")
                     .WithSpannerConfigs(spannerConfig)
                     .WithWaitTimeout(30);

builder.AddDotnetProject("api-example", "../MVFC.Aspire.Helpers.Playground.Api/MVFC.Aspire.Helpers.Playground.Api.csproj")
       .WithReference(spanner)
       .WaitFor(spanner);

await builder.Build().RunAsync();
```

## Configuração de Recursos Emulados

### `SpannerConfig`

| Parâmetro       | Tipo                     | Padrão | Descrição                                     |
| --------------- | ------------------------ | ------ | --------------------------------------------- |
| `ProjectId`     | string                   | —      | ID do projeto GCP.                            |
| `InstanceId`    | string                   | —      | ID da instância.                              |
| `DatabaseId`    | string                   | —      | ID do banco de dados.                         |
| `DdlStatements` | `IReadOnlyList<string>?` | `null` | Instruções DDL a executar após criação do DB. |

## Portas

- **Porta gRPC:** `9010`

## Diagrama de provisionamento

```mermaid
sequenceDiagram
    participant Aspire as .NET Aspire
    participant Container as Spanner Emulator Container
    participant Configurator as Config Processor
    
    Aspire->>Container: Start container (gcr.io/cloud-spanner-emulator/emulator)
    Container-->>Aspire: Ready (gRPC port 9010 available)
    Aspire->>Configurator: Trigger OnResourceReady
    Configurator->>Container: Check/Create Instance
    Configurator->>Container: Check/Create Database
    Configurator->>Container: Execute DDLs
    Configurator-->>Aspire: Provisioning Completed
    Note over Aspire,Configurator: WaitFor observes resource health; provisioning is a separate callback
```

## Métodos Públicos

- `AddGcpSpanner` – adiciona o container do emulador.
- `WithSpannerConfigs` – configura instâncias, bancos de dados e scripts DDL.
- `WithWaitTimeout` – define o prazo compartilhado de resolução do endpoint e provisionamento.
- `WithReference` – liga projetos ao emulador e configura a variável de ambiente `SPANNER_EMULATOR_HOST` automaticamente.

## Imagem padrão

A imagem padrão é fixada por digest SHA256 testado. Para selecionar outra imagem/tag, use `WithDockerImage(image, tag)` ou os métodos de imagem do Aspire; a sobrescrita remove o digest anterior. Consulte os [contratos de execução](../../docs/runtime-contracts.pt-BR.md) para política de atualização, prontidão e limites dos emuladores.

## Requisitos

- .NET 9, 10 ou 11
- Aspire.Hosting 13.6.5 no .NET 9; Aspire.Hosting 17.0.0 no .NET 10/11. Exemplos de AppHost usam `Aspire.Hosting.Dotnet` 17 e `AddDotnetProject`; AppHosts .NET 9 mantêm o modelo `ProjectResource`.
- Docker em execução
- Google.Cloud.Spanner.Data >= 5.12.0 (ou Google.Cloud.Spanner.Admin.Instance.V1)

## Licença

Apache-2.0

## Prazo de provisionamento e recuperação

`WithWaitTimeout` aplica um único prazo à resolução do endpoint, às chamadas administrativas e ao polling de operações. O resultado final da operação concluída é verificado; falhas remotas são propagadas. A expiração gera `TimeoutException` com a causa de cancelamento; o cancelamento do AppHost continua sendo cancelamento.

Cancelar a espera não desfaz operações remotas já aceitas. Na próxima execução, instâncias e bancos existentes são reutilizados. Recursos nunca são excluídos automaticamente, e mudanças de DDL não são aplicadas a bancos existentes. Corridas de criação e outros erros gRPC são propagados, sem serem aceitos silenciosamente.
