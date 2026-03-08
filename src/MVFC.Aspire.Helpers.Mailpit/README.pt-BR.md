# MVFC.Aspire.Helpers.Mailpit

> 🇺🇸 [Read in English](README.md)

Helpers para integração com o emulador de SMTP MailPit em projetos .NET Aspire, facilitando testes de envio de e-mails em ambientes de desenvolvimento.

## Visão Geral

Este projeto permite adicionar e integrar o MailPit como recurso gerenciado em aplicações distribuídas .NET Aspire. Ele simplifica o provisionamento do container MailPit, expõe a interface web para visualização dos e-mails recebidos e fornece métodos de extensão para configuração no AppHost.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.Mailpit`](MVFC.Aspire.Helpers.Mailpit.csproj): Biblioteca de helpers e extensões para MailPit.

## Funcionalidades

- Adiciona o container MailPit à aplicação Aspire.
- Exposição da interface web para visualização dos e-mails recebidos.
- Métodos de extensão para facilitar a configuração no AppHost.
- Permite configuração de porta, número máximo de mensagens, autenticação e persistência opcional dos dados.

## Imagens compatíveis

- `axllent/mailpit`

## Instalação

Adicione o pacote NuGet ao seu projeto AppHost:

```sh
dotnet add package MVFC.Aspire.Helpers.Mailpit
```

## Exemplo de Uso no AppHost

```csharp
var builder = DistributedApplication.CreateBuilder(args);

var mailpit = builder.AddMailpit("mailpit")
    .WithMaxMessages(1000)
    .WithDataFilePath("/data/mailpit.db")
    .WithWebAuth(username: "admin", password: "secret");

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithReference(mailpit)
       .WaitFor(mailpit);

await builder.Build().RunAsync();
```

## Métodos Fluentes

| Método | Descrição |
|---|---|
| `WithDockerImage(image, tag)` | Substitui a imagem Docker utilizada. |
| `WithMaxMessages(max)` | Define o número máximo de mensagens armazenadas. |
| `WithMaxMessageSize(sizeInMb)` | Define o tamanho máximo de cada mensagem em MB. |
| `WithSmtpAuth()` | Habilita autenticação SMTP (accept any + insecure). |
| `WithSmtpHostname(hostname)` | Define o hostname do servidor SMTP. |
| `WithDataFilePath(path)` | Define o caminho do arquivo de persistência dos e-mails. |
| `WithWebAuth(username, password)` | Habilita autenticação na interface web. |
| `WithVerboseLogging()` | Habilita logs detalhados do MailPit. |

## Parâmetros de `AddMailpit`

| Parâmetro | Tipo | Padrão | Descrição |
|---|---|---|---|
| `name` | `string` | — | Nome do recurso. |
| `httpPort` | `int` | `8025` | Porta da interface web. |
| `smtpPort` | `int` | `1025` | Porta do servidor SMTP. |

## Detalhes de Porta e Visualização

- **Porta SMTP**: definida via parâmetro `smtpPort` (padrão: `1025`).
- **Porta Web**: definida via parâmetro `httpPort` (padrão: `8025`).
- **Acesso à interface**: A interface web do MailPit fica disponível em `http://localhost:<httpPort>/`.

## Requisitos
- .NET 9+
- Aspire.Hosting >= 9.5.0

## Licença
Apache-2.0
