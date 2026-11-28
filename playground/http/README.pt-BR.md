# Cenários HTTP do Aspire playground

> 🇺🇸 [Read in English](README.md)

Coleção para [httpyac](https://httpyac.github.io/guide/), com scripts JavaScript, asserções e dependências entre requests. Execute com o AppHost já iniciado e os recursos prontos. Nenhuma configuração do AppHost, TTL, fixture ou CI/CD é alterada por estes arquivos.

## Preparação

A API do playground agora aguarda um primary MongoDB gravável e a conclusão dos dumps por `WaitForDumps(mongo)` antes de iniciar. Essa garantia pertence à configuração do AppHost; os scripts HTTP não populam o MongoDB. As imagens padrão dos emuladores usam digests testados. Consulte os [contratos de execução](../../docs/runtime-contracts.pt-BR.md) para os níveis de prontidão e limites dos emuladores; sucesso local não comprova equivalência com a nuvem.

1. Inicie o playground na raiz do repositório:

   ```powershell
   dotnet run --project playground/MVFC.Aspire.Helpers.Playground.AppHost
   ```

2. Copie do dashboard do Aspire a URL HTTP da API `api-exemplo`. A porta do proxy pode diferir da porta interna 5050.
3. Ajuste as URLs em [variables.http](variables.http), importado automaticamente por todos os cenários. Os valores iniciais são `5050` (API), `7070` (WireMock), `8081` (Keycloak), `15672` (RabbitMQ Management), `8998` (tráfego Apigee, `apigeeUrl`) e `4443` (Cloud Storage, `storageUrl`). Confira os endpoints publicados pelo dashboard. As chamadas Apigee usam `/demo` no endpoint de tráfego, em vez da API de controle na porta `7071`.
4. Os valores de autenticação são dados públicos do seed de desenvolvimento, identificados como `brokerLocalAccount`, `brokerLocalValue`, `identityLocalClient`, `identityClientValue`, `identityLocalAccount`, `identityLocalValue`, `gatewayLocalAccount` e `gatewayLocalValue`. Não coloque credenciais reais nesse arquivo. Os nomes neutros não mudam os parâmetros exigidos pelos protocolos.

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
| [13-apigee.http](13-apigee.http) | Roteamento, headers, Basic Auth, transformação JSON, conversão XML, CORS, métodos bloqueados, observação do cache, health agregado e shared flow | Chama o proxy e popula uma chave exclusiva de cache |
| [14-storage.http](14-storage.http) | Criação de bucket, upload, metadados, listagem por prefixo, download, sobrescrita, atualização de metadados, cópia, exclusão direta e pela API | Cria e remove somente seu próprio bucket `httpyac-<uuid>` e seus objetos |
| [15-apigee-quota-opcional.http](15-apigee-quota-opcional.http) | Loop: cinco chamadas aceitas e uma sexta com HTTP 429 | Altera o contador de quota de um identificador exclusivo; desabilitado por padrão |

### Apigee

Aguarde `GET <apigeeUrl>/demo/health` retornar 200: a API pronta não garante que o proxy já foi implantado. Execute o cenário normal e a quota opcional separadamente:

```powershell
httpyac send playground/http/13-apigee.http --all --bail --output short --output-failed short
httpyac send playground/http/15-apigee-quota-opcional.http --all --bail --var allowApigeeQuota=true --output short --output-failed short
```

O cenário normal verifica Basic Auth inválido e válido com dados públicos de demonstração, sem imprimir headers de autorização. A segunda leitura de cache aceita `HIT` ou `MISS` e compara o corpo armazenado quando houver `HIT`. O emulador local retornou `MISS` nas duas chamadas durante a validação; portanto, a coleção não afirma comprovar reaproveitamento do cache. O cenário não executa rajadas de SpikeArrest nem chama o destino externo `/demo/status`.

A quota usa um único identificador exclusivo em `X-Forwarded-For` nas seis chamadas sequenciais, dentro da janela configurada de um minuto. Não pause o loop, não execute em paralelo e não trate esse header de demonstração como mecanismo de autorização em produção. Execute **httpyac: Reset** antes de repetir o cenário na extensão.

### CRUD Cloud Storage

```powershell
httpyac send playground/http/14-storage.http --all --bail --output short --output-failed short
```

O cenário chama diretamente a JSON API do emulador por `storageUrl` e depois exercita a rota real de download/exclusão do playground por `apiUrl`. Ele gera seu próprio nome de bucket e codifica nomes de objetos com pastas e espaços na URL. Não utiliza o `storageBucket` configurável nem o `bucket-teste` do seed.

As chamadas finais removem a cópia temporária, deixam a API baixar e apagar o objeto restante, verificam que o bucket ficou vazio, removem o bucket e confirmam HTTP 404. A exclusão aceita HTTP 200 (observado no emulador local) ou 204, com leituras posteriores comprovando a remoção. Se `--bail` ou uma interrupção parar o cenário, a limpeza pode não ocorrer: confira o nome gerado na URL da requisição e remova somente esse bucket pela JSON API do emulador. Não há expiração automática configurada.

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
