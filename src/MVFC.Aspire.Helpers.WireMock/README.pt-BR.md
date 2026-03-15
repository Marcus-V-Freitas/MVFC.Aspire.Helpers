# MVFC.Aspire.Helpers.WireMock

> 🇺🇸 [Read in English](README.md)

[![CI](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/Marcus-V-Freitas/MVFC.Aspire.Helpers/branch/main/graph/badge.svg)](https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers)
[![License](https://img.shields.io/badge/license-Apache--2.0-blue)](../../LICENSE)
![Platform](https://img.shields.io/badge/.NET-9%20%7C%2010-blue)
![NuGet Version](https://img.shields.io/nuget/v/MVFC.Aspire.Helpers.WireMock)
![NuGet Downloads](https://img.shields.io/nuget/dt/MVFC.Aspire.Helpers.WireMock)


Helpers para integração do WireMock.Net em projetos .NET Aspire, facilitando o mock de APIs para desenvolvimento, testes e integração.

## Motivação

Fazer mock de APIs HTTP localmente normalmente envolve:

- Rodar o WireMock.Net manualmente ou como um app console separado.
- Espalhar configurações de mock por arquivos JSON ou projetos de teste.
- Sem um lugar claro para gerenciar o ciclo de vida do mock junto com o restante da topologia.

Com o .NET Aspire você pode orquestrar recursos, mas ainda precisa:

- Iniciar/parar o mock junto com a aplicação.
- Configurar endpoints, métodos e autenticação de forma consistente.
- Ligar outros projetos para se comunicarem com o mock.

O `MVFC.Aspire.Helpers.WireMock` resolve isso com:

- `AddWireMock(...)` para rodar o WireMock.Net como servidor embutido no Aspire.
- Uma API fluente para configurar endpoints, autenticação, headers, tipos de corpo e respostas.
- `WithReference(...)` para que os projetos esperem pelo mock e consumam sua URL.

## Visão Geral

Este projeto permite adicionar facilmente um servidor WireMock.Net como recurso gerenciado em aplicações distribuídas .NET Aspire. Ele simplifica o provisionamento, ciclo de vida e exposição de endpoints HTTP mockados, além de permitir configuração personalizada e publicação de eventos de estado/logs.

### Vantagens do WireMock Helper

- Simula APIs externas/localmente para testes e integração.
- Permite definir endpoints, métodos, autenticação e respostas customizadas.
- Facilita o desenvolvimento desacoplado e testes automatizados.
- Gerencia ciclo de vida do recurso WireMock no ambiente Aspire.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.WireMock`](MVFC.Aspire.Helpers.WireMock.csproj): Biblioteca de helpers e extensões para WireMock.Net.

## Funcionalidades

- Adiciona recurso WireMock à aplicação Aspire com ciclo de vida gerenciado.
- Permite configuração detalhada dos endpoints mockados.
- Suporte a autenticação (Bearer, custom headers).
- Configuração de tipos de corpo, headers, status code e erros.
- Publicação de eventos de estado e logs do recurso.

### Imagens compatíveis

- Utiliza o WireMock.Net como servidor embutido (não depende de imagem Docker).

## Instalação

```sh
dotnet add package MVFC.Aspire.Helpers.WireMock
```

## Configuração dos Endpoints

Você pode configurar endpoints mockados com diferentes métodos HTTP, tipos de corpo, autenticação, headers e respostas customizadas.

- **Autenticação Bearer:**

```csharp
server.Endpoint("/api/secure")
       .RequireBearer("mytoken", "Unauthorized", BodyType.String)
       .OnGet(() => ("Secret Data", HttpStatusCode.OK, BodyType.String));
```

- **Headers personalizados:**

```csharp
server.Endpoint("/api/headers")
      .WithResponseHeaders(new() { { "X-Test", ["v1", "v2"] } })
      .OnGet(() => ("Headers OK", HttpStatusCode.OK, BodyType.String));
```

Tipos de corpo suportados: `String`, `Json`, `Bytes`, `FormUrlEncoded`, etc.

## Detalhes de Porta e Visualização

- **Porta**: definida via parâmetro `port` (ex: `8080`).
- **Acesso**: `http://localhost:<porta>/api/...`

## Métodos Públicos

- `AddWireMock` – adiciona o recurso WireMock à aplicação distribuída e permite configurar endpoints.

```csharp
var wireMock = builder.AddWireMock("wireMock", port: 8080, configure: ...);
```

## Exemplo completo de Uso no AppHost

```csharp
using Aspire.Hosting;
using MVFC.Aspire.Helpers.WireMock;

var builder = DistributedApplication.CreateBuilder(args);

var wireMock = builder.AddWireMock("wireMock", port: 8080, configure: (server) =>
{
    server.Endpoint("/api/echo")
          .WithDefaultBodyType(BodyType.String)
          .OnPost<string, string>(body => ($"Echo: {body}", HttpStatusCode.Created, null));

    server.Endpoint("/api/test")
          .WithDefaultBodyType(BodyType.String)
          .OnGet<string>(() => ("Aspire GET OK", HttpStatusCode.OK, null));

    server.Endpoint("/api/secure")
           .RequireBearer("mytoken", "Unauthorized", BodyType.String)
           .OnGet(() => ("Secret Data", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/put")
           .WithDefaultBodyType(BodyType.String)
           .OnPut<string, string>(req => ($"Echo: {req}", HttpStatusCode.Accepted, BodyType.String));

    server.Endpoint("/api/customauth")
        .WithDefaultErrorStatusCode(HttpStatusCode.Forbidden)
        .RequireCustomAuth(req => (req.Headers!.ContainsKey("X-Test"), "Forbidden", BodyType.String))
        .OnGet(() => ("Authorized", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/headers")
        .WithResponseHeaders(new() { { "X-Test", ["v1", "v2"] } })
        .WithResponseHeader("X-Other", "v3")
        .OnGet(() => ("Headers OK", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/error")
        .WithRequestBodyType(BodyType.String)
        .WithDefaultErrorStatusCode((HttpStatusCode)418)
        .OnGet(() => ("I am a teapot", (HttpStatusCode)418, BodyType.String));

    server.Endpoint("/api/delete")
       .WithResponseBodyType(BodyType.String)
       .WithResponseHeader("v1", "v1")
       .WithResponseHeaders(new() { { "v1", ["v2", "v3"] } })
       .WithResponseHeader("v1", "v4")
       .OnDelete<string>(() => (null!, HttpStatusCode.NoContent, null));

    server.Endpoint("/api/form")
        .WithDefaultBodyType(BodyType.FormUrlEncoded)
        .OnPost<Dictionary<string, string>, IDictionary<string, string>>(body => (body, HttpStatusCode.OK, BodyType.FormUrlEncoded));

    server.Endpoint("/api/form-wrong")
       .WithDefaultBodyType(BodyType.FormUrlEncoded)
       .OnPost<string, string>(body => (body, HttpStatusCode.OK, BodyType.FormUrlEncoded));

    server.Endpoint("/api/patch")
        .WithDefaultBodyType(BodyType.String)
        .OnPatch<string, string>(body => ($"Patched: {body}", HttpStatusCode.OK, BodyType.String));

    server.Endpoint("/api/bytes")
        .WithDefaultBodyType(BodyType.Bytes)
        .OnPost<byte[], byte[]>(body => (body, HttpStatusCode.OK, BodyType.Bytes));

    server.Endpoint("/api/unsupported")
        .WithDefaultBodyType((BodyType)999)
        .OnPost<string, string>(_ => ("Not Supported", HttpStatusCode.NotImplemented, null));

    server.Endpoint("/api/json")
        .WithDefaultBodyType(BodyType.Json)
        .OnPost<JsonModel, JsonModel>(body => (body, HttpStatusCode.OK, BodyType.Json));
});

await builder.Build().RunAsync();
```

## Requisitos

- .NET 9+
- Aspire.Hosting >= 9.5.0
- WireMock.Net.minimal >= 1.14.0

## Licença

Apache-2.0
