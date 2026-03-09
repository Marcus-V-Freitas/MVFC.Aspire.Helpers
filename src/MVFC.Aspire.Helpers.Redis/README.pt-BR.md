# MVFC.Aspire.Helpers.Redis

> 🇺🇸 [Read in English](README.md)

Helper para integração com Redis em projetos .NET Aspire, incluindo cache distribuído e Redis Commander UI.

## Motivação

Para desenvolvimento local, o Redis é muitas vezes iniciado via container ad-hoc:

- Sem um lugar claro para centralizar a configuração de senha.
- Sem UI integrada para inspecionar chaves/valores.
- Sem uma forma consistente de montar volumes e preservar estado.

Com o .NET Aspire você pode subir o container do Redis, mas ainda precisa:

- Decidir como expor o Redis para os projetos.
- Configurar o Redis Commander (ou similar) manualmente.
- Manter as connection strings alinhadas entre os serviços.

O `MVFC.Aspire.Helpers.Redis` resolve isso com:

- `AddRedis(...)` para provisionar o Redis.
- `WithPassword(...)`, `WithCommander(...)`, `WithDataVolume(...)` para os cenários mais comuns.
- `project.WithReference(redis)` para passar a connection string via configuração.

## Visão Geral

Este projeto fornece métodos de extensão para facilitar a integração com Redis em projetos .NET Aspire, incluindo cache distribuído e Redis Commander UI.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.Redis`](MVFC.Aspire.Helpers.Redis.csproj): Biblioteca de helpers e extensões para Redis.

## Funcionalidades

- Adiciona um container Redis configurado.
- Suporte ao Redis Commander UI.
- Suporte a persistência de dados via volume Docker (AOF habilitado).
- Suporte a senha.

### Imagens compatíveis

- `redis`
- `rediscommander/redis-commander` (UI)

## Instalação

```sh
dotnet add package MVFC.Aspire.Helpers.Redis
```

## Uso rápido no Aspire (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.Redis;

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

| Método                         | Descrição                                            |
|-------------------------------|------------------------------------------------------|
| `WithDockerImage(image, tag)` | Substitui a imagem Docker utilizada.                 |
| `WithPassword(password)`      | Define a senha do Redis.                             |
| `WithCommander(port?)`        | Adiciona o Redis Commander UI.                       |
| `WithDataVolume(volumeName)`  | Habilita persistência com volume Docker (AOF).       |

## Principais parâmetros do `AddRedis`

- `name`: Nome do recurso Redis.
- `port` *(opcional)*: Porta do Redis (padrão: `6379`).

## Parâmetros opcionais

- **`connectionStringSection`** (opcional):  
  Caminho da configuração que contém a connection string do Redis.  
  Padrão: `"ConnectionStrings:redis"`.

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
