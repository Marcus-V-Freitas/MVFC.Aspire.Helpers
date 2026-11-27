# Contratos de execução e evolução da plataforma

> [Read in English](runtime-contracts.md)

## Prontidão

| Contrato | O que comprova | O que não comprova |
| --- | --- | --- |
| Redis `WithTcpHealthCheck()` + `WaitFor(redis)` | É possível estabelecer uma conexão TCP | Autenticação ou comandos Redis |
| Startup Redis do playground | PING autenticado dentro do prazo configurado | Disponibilidade futura |
| Mongo `WithPrimaryHealthCheck()` + `WaitFor(mongo)` | `hello.isWritablePrimary` é verdadeiro | Conclusão dos dumps |
| Consumidor `WaitForDumps(mongo)` | Primary gravável pronto e todos os dumps concluídos com sucesso | Escrita atômica ou efeitos únicos entre reinicializações do AppHost |
| Playground `/health/ready` | A API concluiu seu startup | Prontidão de todos os emuladores ou callbacks independentes |

`WithReference` fornece a configuração da conexão; não adiciona dependências de startup universalmente. `WaitForDumps` adiciona a verificação de primary e a dependência. Use `WithReference` separadamente para injetar a conexão:

```csharp
var mongo = builder.AddMongoReplicaSet("mongo").WithDumps(dumps);
api.WithReference(mongo).WaitForDumps(mongo);
```

Dumps, referências e espera podem ser configurados em qualquer ordem durante a configuração do AppHost. A execução é compartilhada por recurso Mongo durante uma execução do AppHost. Consumidores simultâneos observam o mesmo resultado. Cancelar um consumidor aguardando não cancela o trabalho compartilhado iniciado pela espera; encerrar o AppHost cancela esse trabalho. Falhas são preservadas e bloqueiam os consumidores sem repetir automaticamente escritas possivelmente parciais. Use `_id` estável e `MongoUpsertDump<T>` quando uma reinicialização precisar reconciliar dados existentes.

O comportamento existente de `WithReference` permanece disponível: sem `WaitForDumps`, os dumps são disparados depois que o primeiro projeto referenciador fica pronto. Consumidores que precisam dos seeds durante o startup devem habilitar a espera. O playground faz isso. Essa espera cria uma dependência real: não faça o startup do Mongo depender do mesmo consumidor.

## Imagens reproduzíveis

Helpers que selecionavam imagens flutuantes `latest` agora aplicam um digest SHA256 testado ao criar o recurso. Isso inclui Cloud Storage, BigQuery, Firestore, Pub/Sub e sua UI, Spanner, Mailpit, MongoDB, Redis e a imagem padrão do Redis Commander. As constantes históricas de tag continuam disponíveis; o padrão efetivo é o digest na anotação da imagem.

Sobrescreva o padrão com `WithDockerImage(image, tag)`, com `WithImage(image, tag)` / `WithImageTag(tag)` do Aspire ou com `WithImageSHA256(digest)`. Na versão suportada do Aspire, alterar a tag remove o digest anterior. Argumentos personalizados de imagem/tag do Redis Commander são preservados.

Os digests iniciais vieram das imagens utilizadas pela base local de integração aprovada em containers Linux/x64. Isso não comprova suporte de cada imagem a todas as arquiteturas. Nomes e digests ficam na classe de valores padrão de cada pacote e devem ser revisados juntos. Atualizações exigem verificação de startup, funcionalidades, sobrescrita e persistência dos serviços com estado. Digest garante identidade; não garante segurança ou suporte. Sua atualização faz parte da manutenção das versões.

Não use uma atualização de imagem do Mongo para migrar implicitamente um volume existente. Verifique o caminho de atualização suportado e faça backup; reverter o pacote não reverte o formato dos dados.

## Avaliação do Aspire — 2026-11-27

O repositório usa Aspire 13.6.5 e publica assets `net9.0`, `net10.0` e `net11.0`. O manifesto NuGet oficial de Aspire.Hosting 17.0.0 contém apenas um grupo de dependências `net10.0`. Uma atualização uniforme conflita com o alvo atual `net9.0`. Esta entrega preserva frameworks e dependências, evitando remover suporte implicitamente ou manter duas implementações permanentes.

O suporte do Aspire 13.6 terminou em 18/11/2026. Sua manutenção é uma restrição conhecida, não uma estratégia permanente. A próxima atualização de plataforma deverá escolher entre remover `net9.0` em uma versão principal da biblioteca ou testar explicitamente dependências condicionais. A opção escolhida exige validação binária e comportamental.

Uma comparação isolada do modelo com Aspire.Hosting.MongoDB 13.6.5 confirmou que `WithReplicaSet()` experimental já existe. Ela cria `MongoDBServerResource` com endpoint `tcp`; este helper expõe `MongoReplicaSetResource` com endpoint `mongodb`. A substituição não é transparente para consumidores desses tipos ou endpoints. Ciclo de inicialização, autenticação e volumes também precisam de testes antes da migração; a comparação de modelos não comprova equivalência em execução. Dumps e upserts continuam sendo capacidades a preservar quando agregarem valor.

Fontes: [Aspire 17](https://aspire.dev/whats-new/aspire-17/), [ciclo de suporte](https://aspire.dev/support/), [integração Mongo](https://aspire.dev/integrations/databases/mongodb/mongodb-host/), [manifesto Aspire.Hosting 17](https://api.nuget.org/v3-flatcontainer/aspire.hosting/17.0.0/aspire.hosting.nuspec).

## Fidelidade e validação

Validação local pelo Cake em 27/11/2026: a base passou 724 testes, com 100% das linhas (2433/2433) e 96% dos branches (625/651). Depois das mudanças, passaram 757 testes, com 100% das linhas (2471/2471) e 96% dos branches (633/659), sem testes ignorados, avisos ou erros de build. A compilação cobriu os alvos .NET 9/10/11 existentes; o projeto de testes executa em .NET 11. Esse registro corresponde à execução local, não a um novo resultado do CI remoto.

Os testes locais comprovam o comportamento dos helpers nas imagens selecionadas. Não comprovam equivalência com os serviços gerenciados na nuvem. Mensageria local funcionando não valida IAM ou políticas de dead letter em produção. A coleção HTTP do Apigee registra o comportamento observado do cache sem prometer HIT.

As verificações normais continuam no Cake existente. A compatibilidade de APIs de pacotes permanece uma verificação local opcional por [verify-compatibility.ps1](../eng/verify-compatibility.ps1), complementar aos testes comportamentais. Não foi adicionada etapa de compatibilidade no CI nem exigência automática de testes na nuvem.

Contratos dependentes da nuvem devem ser descritos e validados em ambiente autorizado próprio, com limpeza, credenciais e controle de custos, antes de fundamentar garantias em produção. Adoção externa é uma questão de evidência separada e não altera esses contratos de execução.
