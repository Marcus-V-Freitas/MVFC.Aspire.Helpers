# MVFC.Aspire.Helpers.Redis

Helper para integração com Redis em projetos .NET Aspire, incluindo cache distribuído e Redis Commander UI.

## Visão Geral

Este projeto fornece métodos de extensão para facilitar a integração com Redis em projetos .NET Aspire, incluindo cache distribuído e Redis Commander UI.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.Redis`](MVFC.Aspire.Helpers.Redis.csproj): Biblioteca de helpers e extensões para Redis.

## Funcionalidades

- Adiciona um container Redis configurado.
- Suporta Redis Commander UI.
- Suporta persistência de dados.
- Suporta senha.

## Imagens compatíveis:
 - `redis`

## Instalação

```bash
dotnet add package MVFC.Aspire.Helpers.Redis
```

## Exemplo de Uso no AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var redisConfig = new RedisConfig(
    WithCommander: true, 
    VolumeName: "redis-data");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithRedis(builder, name: "redis", redisConfig: redisConfig);

await builder.Build().RunAsync();
```

## Principais parâmetros

- `Port`: Porta do Redis (padrão: null para porta aleatória)
- `Password`: Senha para autenticação no Redis
- `WithCommander`: Booleano para habilitar o Redis Commander UI.
- `CommanderPort`: Porta do Redis Commander (padrão: null para porta aleatória)
- `VolumeName`: String para nome do volume de persistência.

## Outros parâmetros Opcionais importantes:

- **connectionStringSection** (Opcional): Define o caminho da variável de ambiente ou configuração que contém a string de conexão do Redis. O padrão é "ConnectionStrings:redis". Cada `:` indica um nível/seção dentro do arquivo `appsettings.json`, permitindo acessar configurações aninhadas, por exemplo:

```json
{
  "ConnectionStrings": {
    "redis": "localhost:6379"
  }
}
```

## Métodos Públicos

- `WithRedis`: Adiciona um recurso Redis ao projeto Aspire.
- `AddRedis`: Adiciona um recurso Redis ao projeto Aspire.
- `WaitForRedis`: Aguarda até que o recurso Redis esteja disponível antes de iniciar o projeto Aspire.

## Requisitos
- .NET 9+
- Aspire.Hosting >= 9.5.0

## Licença
Apache-2.0  