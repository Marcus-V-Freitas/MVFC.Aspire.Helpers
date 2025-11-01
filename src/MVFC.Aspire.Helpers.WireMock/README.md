# MVFC.Aspire.Helpers.WireMock

Helpers para integração do WireMock.Net em projetos .NET Aspire, facilitando o mock de APIs para desenvolvimento, testes e integração.

## Visão Geral

Este projeto permite adicionar facilmente um servidor WireMock.Net como recurso gerenciado em aplicações distribuídas .NET Aspire. Ele simplifica o provisionamento, ciclo de vida e exposição de endpoints HTTP mockados, além de permitir configuração personalizada e publicação de eventos de estado/logs.

## Vantagens do WireMock Helper

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

## Imagens compatíveis

- Utiliza o WireMock.Net como servidor embutido (não depende de imagem Docker).

## Instalação

Adicione o pacote NuGet ao seu projeto AppHost:

```shell
 dotnet add package MVFC.Aspire.Helpers.WireMock
```

## Configuração dos Endpoints

Você pode configurar endpoints mockados com diferentes métodos HTTP, tipos de corpo, autenticação, headers e respostas customizadas. Exemplos:

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

- **Tipos de corpo suportados:**  
  String, JSON, Bytes, FormUrlEncoded, etc.

## Detalhes de Porta e Visualização

- **Porta padrão:** Definida via parâmetro `port` (exemplo: `8080`).
- **Acesso:**  
  Os endpoints mockados ficam disponíveis em `http://localhost:<porta>/api/...`

## Métodos Públicos

- **AddWireMock**  
  Adiciona o recurso WireMock à aplicação distribuída, permitindo configuração dos endpoints.

  ```csharp
  var wireMock = builder.AddWireMock("wireMock", port: 8080, configure: ...);
  ```

- **Configuração personalizada**  
  Permite definir endpoints, autenticação, tipos de corpo, headers e respostas conforme necessidade.

## Exemplo completo de Uso no AppHost

```csharp
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