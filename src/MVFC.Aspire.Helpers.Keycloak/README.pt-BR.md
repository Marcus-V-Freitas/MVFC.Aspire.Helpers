# MVFC.Aspire.Helpers.Keycloak

> 🇺🇸 [Read in English](README.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.Keycloak)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.Keycloak)


Este pacote fornece métodos de extensão para o **.NET Aspire**, facilitando a integração, configuração e inicialização de um servidor **Keycloak** para gerenciar identidade e acesso no ambiente local.

Com ele, você pode não apenas inicializar um container Keycloak, como também injetar configurações (como `BaseUrl`, `Realm`, `ClientId`, etc.) e importar realms customizados (via seeds JSON) em tempo de desenvolvimento.

## Motivação

O Keycloak é poderoso, mas montar um ambiente local costuma ser doloroso:

- Comandos docker enormes com variáveis de ambiente e flags de import.
- Import manual de realms, clients e usuários a partir de arquivos JSON.
- Repetição do mesmo setup para cada projeto novo ou demo.

Com o .NET Aspire você pode definir o container Keycloak, mas ainda precisa:

- Controlar credenciais de admin, portas e volumes.
- Definir uma forma repetível de semear realms/clients/usuários.
- Garantir que as aplicações esperem o Keycloak ficar pronto.

O `MVFC.Aspire.Helpers.Keycloak` transforma isso em uma API pequena e opinativa:

- `AddKeycloak(...)` sobe o Keycloak com defaults adequados para desenvolvimento.
- `WithSeeds(...)`, `WithDataBindMount(...)` centralizam as preocupações específicas do Keycloak.
- `project.WithReference(keycloak, ...)` faz o projeto esperar pelo Keycloak e o configura com as dependências corretas.

## Recursos e Funcionalidades

- **Server Embutido:** Inicia um container Keycloak oficial (atualmente `quay.io/keycloak/keycloak:26.1.1`).
- **Autenticação Padrão:** Opcional configuração com um `ClientId` pré-definido e seeds para desenvolvedor.
- **Porta Personalizável:** Mapeamento de portas fixo ou dinâmico pelo Aspire.
- **Injeção Transparente:** Adiciona endpoints de autenticação aos projetos `.WithReference()`, configurando URLs, realms e ClientIds como variáveis de ambiente.
- **Volume Persistente:** Opcionalmente salva o estado e as bases de dados internas.
- **Importação Dinâmica de Realm (Seeds):** Inicia seu ambiente de desenvolvimento populando Realms, Clients, Roles e Users via configuração de código.

## Instalação

Adicione o pacote ao projeto principal do AppHost do seu .NET Aspire:

```sh
dotnet add package MVFC.Aspire.Helpers.Keycloak
```

*(Lembre-se de configurar a dependência para outros serviços, como as APIs que usarão as variáveis provisionadas!)*

## Uso rápido no Aspire (AppHost)

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.Keycloak;

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder.AddKeycloak("keycloak", port: 9000)
    .WithSeeds(new()
    {
        Realm = "meu-app-realm",
        Clients = [
            new() { ClientId = "minha-api", Secret = "api-secret-1234", DisableAuth = true }
        ],
        Roles = ["Admin", "User"],
        Users = [
            new() { Username = "admin", Password = "123", Roles = ["Admin", "User"] }
        ]
    });

var api = builder.AddProject<Projects.Minha_Api>("api")
    .WithReference(keycloak, realmName: "meu-app-realm", clientId: "minha-api", clientSecret: "api-secret-1234")
    .WaitFor(keycloak);

builder.Build().Run();
```

## Usando a configuração injetada na API

Por padrão, ao usar `.WithReference(keycloak)`, o Aspire injeta as seguintes chaves no `IConfiguration`:

| Chave de Configuração    | Descrição                                              | Exemplo                 |
|--------------------------|--------------------------------------------------------|-------------------------|
| `Keycloak:BaseUrl`       | URL completa de acesso ao container Keycloak.          | `http://localhost:9000` |
| `Keycloak:Realm`         | Nome do Realm propagado do AppHost.                    | `meu-app-realm`         |
| `Keycloak:ClientId`      | ID do cliente OAuth/OpenID.                            | `minha-api`             |
| `Keycloak:ClientSecret`  | (Opcional) Secret vinculada ao ClientId.               | `api-secret-1234`       |

Exemplo no `Program.cs` da sua API:

```csharp
var baseUrl  = builder.Configuration["Keycloak:BaseUrl"];
var realm    = builder.Configuration["Keycloak:Realm"];
var clientId = builder.Configuration["Keycloak:ClientId"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // desenvolvimento
        options.Audience = clientId;
        options.MetadataAddress = $"{baseUrl}/realms/{realm}/.well-known/openid-configuration";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = $"{baseUrl}/realms/{realm}",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = clientId
        };
    });

builder.Services.AddAuthorization();
```

## Opções do `AddKeycloak()`

| Parâmetro   | Tipo     | Descrição                                                                            | Padrão       |
|------------|----------|--------------------------------------------------------------------------------------|--------------|
| `name`     | string   | Nome do recurso no framework do Aspire.                                             | `"keycloak"` |
| `userName` | string   | Usuário administrador Master do Keycloak.                                           | `admin`      |
| `password` | string   | Senha administrativa Master do Keycloak.                                            | `admin`      |
| `port`     | int?     | Porta fixa exposta para localhost (ex: 9000). Se nulo, o Aspire resolve.            | `null`       |
| `dataBound`| bool     | Se verdadeiro, aplica `.WithDataBindMount()` criando volume de estado.              | `false`      |

## Extensões Adicionais

- `.WithSeeds(KeycloakRealmSeed)` – constrói e importa um JSON de realm via `/opt/keycloak/data/import/`.
- `.WithRealmImport(importPath)` – carrega JSONs de realm estáticos de uma pasta e os importa no container.

## Licença

Apache-2.0
