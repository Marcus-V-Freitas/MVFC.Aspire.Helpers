# MVFC.Aspire.Helpers.Mailpit

Helpers para integração com o emulador de SMTP MailPit em projetos .NET Aspire, facilitando testes de envio de e-mails em ambientes de desenvolvimento.

## Visão Geral

Este projeto permite adicionar e integrar o MailPit como recurso gerenciado em aplicações distribuídas .NET Aspire. Ele simplifica o provisionamento do container MailPit, expõe a interface web para visualização dos e-mails recebidos e fornece métodos de extensão para configuração no AppHost.

## Estrutura do Projeto

- [`MVFC.Aspire.Helpers.Mailpit`](MVFC.Aspire.Helpers.Mailpit.csproj): Biblioteca de helpers e extensões para MailPit.

## Funcionalidades

- Adiciona o container MailPit à aplicação Aspire.
- Exposição da interface web para visualização dos e-mails recebidos.
- Métodos de extensão para facilitar a configuração no AppHost.
- Permite configuração de porta e persistência opcional dos dados.

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

builder.AddProject<Projects.MVFC_Aspire_Helpers_Playground_Api>("api-exemplo")
       .WithMailpit(builder, name: "mailpit");

await builder.Build().RunAsync();
```
## Parâmetros de configuração

### MailPitConfig

- `HttpPort`: Porta da interface web do MailPit.
- `SmtpPort`: Porta do servidor SMTP.
- `MaxMessages`: Quantidade máxima de mensagens armazenadas.
- `DataFilePath`: Caminho para persistência dos dados dos e-mails.
- `SmtpAuthAcceptAny`: Permite autenticação SMTP com qualquer usuário/senha.
- `SmtpAuthAllowInsecure`: Permite autenticação SMTP em conexões inseguras.
- `EnableWebAuth`: Habilita autenticação na interface web.
- `WebAuthUsername`: Usuário para autenticação web.
- `WebAuthPassword`: Senha para autenticação web.
- `ImageName`: Nome da imagem Docker utilizada.
- `ImageTag`: Tag da imagem Docker utilizada.
- `VerboseLogging`: Habilita logs detalhados do MailPit.
- `MaxMessageSize`: Tamanho máximo permitido para cada mensagem (em MB).
- `SmtpHostname`: Hostname do servidor SMTP.

## Detalhes de Porta e Visualização

- **Porta SMTP**: Definida via parâmetro smtpPort (exemplo: `1025`).
- **Porta Web**: Definida via parâmetro webPort (exemplo: `8025`).
- **Acesso à interface**: A interface web do MailPit fica disponível em `http://localhost:<webPort>/`

## Métodos Públicos

- **AddMailpit**  
Adiciona o recurso MailPit à aplicação distribuída.

- **WithMailpit**  
Integra o recurso MailPit ao projeto, configurando dependências e variáveis de ambiente.

## Requisitos
- .NET 9+
- Aspire.Hosting >= 9.5.0

## Licença
Apache-2.0