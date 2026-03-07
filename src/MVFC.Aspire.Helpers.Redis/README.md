# MVFC.Aspire.Helpers.Redis

Helper para integração com Redis em projetos .NET Aspire, incluindo cache distribuído e Redis Commander UI.

## Visão Geral

Este projeto fornece métodos de extensão para facilitar a integração com Redis em projetos .NET Aspire, incluindo cache distribuído e Redis Commander UI.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.Redis`](MVFC.Aspire.Helpers.Redis.csproj): Biblioteca de helpers e extensões para Redis.

## Funcionalidades

- Adiciona um container Redis configurado.
- Suporte ao Redis Commander UI.
- Suporte a persistência de dados via volume Docker (AOF habilitado).
- Suporte a senha.

## Imagens compatíveis:
 - `redis`
 - `rediscommander/redis-commander` (UI)

## Instalação

```bash
dotnet add package MVFC.Aspire.Helpers.Redis
```

## Exemplo de Uso no AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis")
    .WithPassword("minha-senha")
    .WithCommander()
    .WithDataVolume("redis-data");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithReference(redis)
       .WaitFor(redis);

await builder.Build().RunAsync();
```

## Métodos Fluentes

| Método | Descrição |
|---|---|
| `WithDockerImage(image, tag)` | Substitui a imagem Docker utilizada. |
| `WithPassword(password)` | Define a senha do Redis. |
| `WithCommander(port?)` | Adiciona o Redis Commander UI. |
| `WithDataVolume(volumeName)` | Habilita persistência com volume Docker (AOF). |

## Principais parâmetros do `AddRedis`

- `name`: Nome do recurso Redis.
- `port` *(Opcional)*: Porta do Redis (padrão: `6379`).

## Outros parâmetros Opcionais importantes:

- **connectionStringSection** (Opcional): Define o caminho da variável de ambiente ou configuração que contém a string de conexão do Redis. O padrão é `"ConnectionStrings:redis"`. Cada `:` indica um nível/seção dentro do arquivo `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "redis": "localhost:6379"
  }
}
```

## Detalhes de Porta

- **Porta Redis**: definida via parâmetro `port` (padrão: `6379`).
- **Porta Redis Commander**: aleatória por padrão; pode ser definida via parâmetro `commanderPort` em `WithCommander`.

## Requisitos
- .NET 9+
- Aspire.Hosting >= 9.5.0

## Licença
Apache-2.0
