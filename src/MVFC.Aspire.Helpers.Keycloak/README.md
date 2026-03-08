# MVFC.Aspire.Helpers.Keycloak

Este pacote fornece métodos de extensão para o **.NET Aspire**, facilitando a integração, configuração e inicialização de um servidor **Keycloak** para gerenciar identidade e acesso no ambiente local. 

Com ele, você pode não apenas inicializar um container Keycloak, como também injetar configurações (como `BaseUrl`, `Realm`, `ClientId`, etc.) e importar realms customizados (via seeds JSON) em tempo de desenvolvimento.

---

## Recursos e Funcionalidades

- **Server Embutido:** Inicia um container Keycloak oficial (atualmente `quay.io/keycloak/keycloak:26.1.1`).
- **Autenticação Padrão:** Opcional configuração com um ClientId pré-definido e seeds para desenvolvedor.
- **Porta Personalizável:** Mapeamento de portas fixo ou dinâmico pelo Aspire.
- **Injeção Transparente:** Adiciona endpoints de autenticação aos projetos `.WithReference()`, configurando URLs, realms e ClientIds como variáveis de ambiente no modelo correto.
- **Volume Persistente:** Opcionalmente salva o estado e as bases de dados H2.
- **Importação Dinâmica de Realm (Seeds):** Facilita iniciar seu ambiente de desenvolvimento populando Realms, Clients, Roles e Users via configuração de código. 

---

## Como instalar

Adicione o pacote ao projeto principal do AppHost do seu .NET Aspire:

```bash
dotnet add package MVFC.Aspire.Helpers.Keycloak
```

*(Lembre-se de configurar a dependência para outros serviços, como as APIs que usarão as variáveis provisionadas!)*

---

## Como utilizar

### 1. Registrando o Keycloak no `AppHost`

No projeto de infraestrutura do seu Aspire (o `AppHost`), registre o Keycloak e opcionalmente adicione um _seed_ (dados iniciais para seu ambiente de desenvolvimento):

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Adicionando um container Keycloak básico
var keycloak = builder.AddKeycloak("keycloak", port: 9000)
    // Exemplo: criando um Realm automaticamente com clients e roles via código
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

// Associando o Identity Provider a uma API
var api = builder.AddProject<Projects.Minha_Api>("api")
    // Em APIs rodando Aspire, ao referenciar este componente nós geramos automaticamente 
    // Keycloak:BaseUrl, Keycloak:Realm e etc na aplicação referenciada
    .WithReference(keycloak, realmName: "meu-app-realm", clientId: "minha-api", clientSecret: "api-secret-1234")
    .WaitFor(keycloak);

builder.Build().Run();
```

### 2. Configurando Autenticação na `.Api`

Você pode referenciar os dados gerados pelo Keycloak do seu `AppHost` diretamente usando pacotes oficiais (como `Microsoft.AspNetCore.Authentication.JwtBearer`). 
Por padrão, ao usar `.WithReference(keycloak)` o Aspire injeta nativamente as seguintes configurações no seu `IConfiguration`:

| Chave de Configuração | Descrição | Exemplo |
| :--- | :--- | :--- |
| `Keycloak:BaseUrl` | URL completa interna/externa de acesso ao container Keycloak. | `http://localhost:9000` |
| `Keycloak:Realm` | O nome do Realm configurado e propagado do AppHost. | `meu-app-realm` |
| `Keycloak:ClientId` | O ID do cliente de fluxo OAuth/OpenID conectado. | `minha-api` |
| `Keycloak:ClientSecret` | (Opcional) A secret mapeada vinculada ao ClientId. | `api-secret-1234` |

Exemplo no seu `Program.cs` ou `InstanceHelpers.cs` da API:

```csharp
var baseUrl = builder.Configuration["Keycloak:BaseUrl"];
var realm   = builder.Configuration["Keycloak:Realm"];
var clientId= builder.Configuration["Keycloak:ClientId"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Em desenvolvimento
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

---

## Opções do `AddKeycloak()`

| Parâmetro   | Tipo     | Descrição                                                                            | Padrão                                |
| ----------- | -------- | ------------------------------------------------------------------------------------ | ------------------------------------- |
| `name`      | `string` | Nome do recurso no framework do Aspire.                                              | `"keycloak"`                          |
| `userName`  | `string` | Usuário administrador Master do Keycloak.                                            | `admin`                               |
| `password`  | `string` | Senha administrativa Master do Keycloak.                                             | `admin`                               |
| `port`      | `int?`   | Porta fixa (opcional) exposta para localhost (ex: 9000). Caso Nulo o Aspire resolve. | `null`                                |
| `dataBound` | `bool`   | Se verdadeiro irá aplicar `.WithDataBindMount()` criando volume de estado.           | `false`                               |

## Extensões Adicionais

- `.WithSeeds(KeycloakRealmSeed)`: Utiliza o padrão builder de configuração para provisionar um JSON `[my realm]-realm.json` e importá-lo no boot através do diretório `/opt/keycloak/data/import/`.
- `.WithRealmImport(importPath)`: Carrega e usa uma sub-pasta ou pasta externa do Host para importar configurações de Realm estáticas via JSON diretamente na imagem Container.
