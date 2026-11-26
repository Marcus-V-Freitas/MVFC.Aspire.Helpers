# Cenários HTTP do Aspire playground

> 🇺🇸 [Read in English](README.md)

Coleção para [httpyac](https://httpyac.github.io/guide/), com scripts JavaScript, asserções e dependências entre requests. Execute com o AppHost já iniciado e os recursos prontos. Nenhuma configuração do AppHost, TTL, fixture ou CI/CD é alterada por estes arquivos. Apigee permanece fora deste escopo.

## Preparação

1. Inicie o playground na raiz do repositório:

   ```powershell
   dotnet run --project playground/MVFC.Aspire.Helpers.Playground.AppHost
   ```

2. Copie do dashboard do Aspire a URL HTTP da API `api-exemplo`. A porta do proxy pode diferir da porta interna 5050.
3. Ajuste as URLs em [variables.http](variables.http), importado automaticamente por todos os cenários. Os valores iniciais são `5050` (API), `7070` (WireMock), `8081` (Keycloak) e `15672` (RabbitMQ Management). Confira os endpoints publicados pelo dashboard.
4. Os valores de autenticação são dados públicos do seed de desenvolvimento, identificados como `brokerLocalAccount`, `brokerLocalValue`, `identityLocalClient`, `identityClientValue`, `identityLocalAccount` e `identityLocalValue`. Não coloque credenciais reais nesse arquivo. Os nomes neutros não mudam os parâmetros exigidos pelos protocolos.

## VS Code

Antes de enviar requisições, confirme que `GET <apiUrl>/health/ready` retorna HTTP 200. A API inicia independentemente da UI administrativa do Pub/Sub. Cenários que chamam RabbitMQ Management ou Keycloak diretamente também exigem que esses endpoints estejam disponíveis. Se um cliente de infraestrutura exceder o prazo de inicialização, consulte o erro de startup da API; configure `Infrastructure__StartupTimeoutSeconds` para alterar o padrão de 30 segundos por cliente Redis/RabbitMQ. Uma API pronta não garante que todos os callbacks de dados de exemplo tenham terminado.

Com a extensão **httpyac** instalada, abra um cenário `.http` e execute **Send All Requests**. Não é necessário selecionar ambiente ou carregar arquivos `.env`: os valores vêm de [variables.http](variables.http), via `@import`. Cada região tem um nome para execução individual. Requests encadeados usam `@ref`; para repetir um cenário completo, use **httpyac: Reset** e execute novamente.

## Linha de comando

Instale o CLI uma vez, se necessário:

```powershell
npm install -g httpyac
```

Execute na raiz do repositório:

```powershell
# Conectividade: MongoDB, Firestore e Spanner
httpyac send playground/http/01-smoke.http --all --bail --output short

# Variável sobrescrita sem editar os arquivos
httpyac send playground/http/02-redis.http --all --bail --var apiUrl=http://localhost:5050 --output short

# Todos os arquivos, sequencialmente
httpyac send "playground/http/[0-9]*.http" --all --bail --output short --output-failed short

# Apenas um serviço
httpyac send playground/http/03-rabbitmq.http --all --bail --output short --output-failed short
```

A extensão funciona sem instalar o CLI. Evite execução paralela de requests encadeados. Use saída `short` também em falhas para não imprimir tokens ou headers de autenticação.

## Cenários e efeitos

| Arquivo | O que comprova | Efeito nos dados |
| --- | --- | --- |
| [01-smoke.http](01-smoke.http) | Conectividade MongoDB, Firestore e Spanner | Somente leitura |
| [02-redis.http](02-redis.http) | Chave inexistente, gravação e leitura do valor exato | Cria uma chave `httpyac-<uuid>`; não há rota de exclusão/TTL na API |
| [03-rabbitmq.http](03-rabbitmq.http) | Fila/binding isolados, publicação pela API, consumo exato e fila vazia | Cria e remove somente a fila `httpyac-<uuid>`; expira por inatividade em 5 minutos se a execução parar |
| [04-firestore.http](04-firestore.http) | Criação e leitura do documento pela identidade retornada | Mantém o usuário criado; a API não possui DELETE |
| [05-spanner.http](05-spanner.http) | Criação e leitura do usuário com nome exclusivo | Mantém o usuário criado; a API não possui DELETE |
| [06-bigquery.http](06-bigquery.http) | Dataset e usuários Alice/Bob do seed | Somente leitura |
| [07-pubsub.http](07-pubsub.http) | Publicação real e simulação do receptor push com envelope base64 | Publica uma mensagem e chama diretamente o receptor |
| [08-keycloak.http](08-keycloak.http) | 401 sem token, obtenção de token, acesso autorizado e senha inválida | Obtém sessão/token do usuário admin de desenvolvimento |
| [09-gotenberg.http](09-gotenberg.http) | Conversão de HTML e assinatura `%PDF-` | Recebe PDF em memória |
| [10-mailpit.http](10-mailpit.http) | Envio via SMTP local | Acrescenta um email capturado no Mailpit; confira o assunto `httpyac-<uuid>` na UI |
| [11-wiremock.http](11-wiremock.http) | GET/POST/PUT/PATCH/DELETE, JSON, headers e autenticação dos mocks; callback da API | Chamadas aos mocks e simulação de callback |
| [12-storage-opcional.http](12-storage-opcional.http) | Rota de download/exclusão do bucket | **Apaga arquivos do bucket selecionado; desabilitado por padrão** |

### RabbitMQ

O cenário não altera nem consome `test-queue`, `empty-queue` ou `dlq`. Usa `test-exchange`, uma routing key `httpyac.<uuid>` e uma fila exclusiva sem TTL de mensagem. Assim valida o caminho publicar/consumir sem depender das configurações de demonstração da `test-queue`. A Management API precisa estar disponível e o usuário deve poder criar filas/bindings. O parâmetro `x-expires` é expiração da fila por inatividade, não TTL das mensagens. Se uma asserção falhar, `--bail` interrompe a execução e a expiração cuida da fila temporária.

### Pub/Sub e callbacks

O endpoint de publicação envia a mensagem fixa implementada na API. A chamada direta ao receptor valida o contrato HTTP e o envelope; não prova que o emulador entregou o push. O receptor atual não oferece um endpoint para consultar entregas recebidas. Os arquivos não afirmam validar esse comportamento ponta a ponta.

### Storage opcional

A rota GET do playground também apaga os objetos que lê. Ela continua desabilitada inclusive ao executar todos os cenários. Para exercitá-la, use um bucket descartável e habilite explicitamente:

```powershell
httpyac send playground/http/12-storage-opcional.http --all --bail --var allowStorageDelete=true storageBucket=meu-bucket-descartavel --output short
```

Para resetar somente dados da simulação, remova suas chaves/usuários por ferramentas do respectivo emulador; não apague volumes ou bancos compartilhados para executar a coleção.
