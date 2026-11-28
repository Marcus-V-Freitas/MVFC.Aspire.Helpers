# Runtime contracts and platform evolution

> [Leia em Português](runtime-contracts.pt-BR.md)

## Readiness

| Contract | What it proves | What it does not prove |
| --- | --- | --- |
| Redis `WithTcpHealthCheck()` + `WaitFor(redis)` | A TCP connection can be established | Authentication or successful Redis commands |
| Playground Redis startup | An authenticated PING succeeds within the configured budget | Future availability |
| Mongo `WithPrimaryHealthCheck()` + `WaitFor(mongo)` | `hello.isWritablePrimary` is true | Dump completion |
| Consumer `WaitForDumps(mongo)` | A writable primary is ready and all configured dumps completed successfully | Atomic seed writes or exactly-once effects across AppHost restarts |
| Playground `/health/ready` | The API has completed startup | Every independently configured emulator or callback is ready |

`WithReference` supplies connection configuration; it does not universally add startup dependencies. `WaitForDumps` adds its own primary check and dependency. Use `WithReference` separately for connection injection:

```csharp
var mongo = builder.AddMongoReplicaSet("mongo").WithDumps(dumps);
api.WithReference(mongo).WaitForDumps(mongo);
```

Dump configuration, references and the wait can appear in any order during AppHost configuration. Dump execution is shared per Mongo resource within one AppHost lifetime. Concurrent consumers observe the same outcome. Canceling one waiting consumer does not cancel shared work started by the wait; application shutdown cancels that work. Failures are retained and block waiting consumers without automatic replay of potentially partial writes. Use stable `_id` values and `MongoUpsertDump<T>` when restarting must reconcile existing data.

The existing `WithReference` behavior remains available: without `WaitForDumps`, dumps are triggered after the first referencing project becomes ready. A consumer that needs seeds during startup must opt into the wait. The playground does so. A wait adds a real dependency: never make Mongo startup depend on the same consumer.

## Reproducible container images

Helpers that previously selected floating `latest` images now apply a tested SHA256 digest when creating the resource. This includes Cloud Storage, BigQuery, Firestore, Pub/Sub and its UI, Spanner, Mailpit, MongoDB, Redis and the default Redis Commander image. The historical tag constants remain available; the effective default is the digest on the container image annotation.

Use the existing `WithDockerImage(image, tag)`, Aspire `WithImage(image, tag)` / `WithImageTag(tag)` or `WithImageSHA256(digest)` to override a default. Changing the tag clears the previous digest in the supported Aspire version. Custom Redis Commander image/tag arguments are preserved.

The initial digests were taken from the images used by the successful local integration baseline on Linux containers/x64. This is not a claim that every image supports every architecture. Digests and image names live in each package's defaults class; review them together. Updating a digest requires startup and functional checks, override tests and persistence checks for stateful services. A digest gives identity, not a security or support guarantee; schedule maintenance as part of normal releases.

Do not use a Mongo image upgrade to migrate an existing volume implicitly. Verify the server's supported upgrade path and back up data first; reverting a package does not revert stored data formats.

## Aspire compatibility matrix — 2026-11-27

The repository publishes `net9.0`, `net10.0` and `net11.0` assets from shared source files. Central dependency selection uses Aspire 13.6.5 for `net9.0` and Aspire 17.0.0 for `net10.0`/`net11.0`. The official Aspire.Hosting 17.0.0 NuGet manifest contains only a `net10.0` dependency group, so the .NET 9 asset cannot reference it directly.

The playground uses `Microsoft.NET.Sdk` with `Aspire.Hosting.AppHost` and `Aspire.Hosting.Dotnet` 17.0.0 and path-based `AddDotnetProject`. Generic `WithReference<TResource>` and `WaitForDumps<TResource>` overloads support that model. The original `ProjectResource` signatures remain forwarding entry points so already-compiled callers can still bind; there are no obsolete API calls or diagnostic suppressions in the migrated startup code. Existing unit tests also exercise those original signatures using unstarted project resources, while functional tests exercise the new model.

Version 13.0.0 is a major library release because adopting its .NET 10/11 assets requires upgrading Aspire dependencies in the consuming AppHost to 17. A .NET 10/11 AppHost can still orchestrate .NET 9 workloads: the workload target and the hosting package target are separate. Aspire 13.6 support ended on 2026-11-18; retaining it for .NET 9 is an explicit maintenance constraint, not an assertion of upstream support.

An isolated model probe against Aspire.Hosting.MongoDB 13.6.5 confirmed that native experimental `WithReplicaSet()` is already available. It creates `MongoDBServerResource` with endpoint `tcp`; this helper exposes `MongoReplicaSetResource` with endpoint `mongodb`. Replacing the implementation is not a transparent substitution for callers that use resource types or endpoints. Native lifecycle/authentication/volume behavior must also be tested before migration; the model probe does not establish runtime equivalence. Preserve dump/upsert capabilities where they still add value.

References: [Aspire 17 release](https://aspire.dev/whats-new/aspire-17/), [support lifecycle](https://aspire.dev/support/), [Mongo integration](https://aspire.dev/integrations/databases/mongodb/mongodb-host/), [Aspire.Hosting 17 manifest](https://api.nuget.org/v3-flatcontainer/aspire.hosting/17.0.0/aspire.hosting.nuspec).

## Emulator fidelity and verification

Local Cake verification on 2026-11-27: before the platform migration, 757 tests passed with 100% line coverage (2471/2471) and 96% branch coverage (633/659). After the Aspire 17 migration, 761 tests passed with 100% line coverage (2601/2601) and 96.1% branch coverage (641/667), with no skipped tests, build warnings or errors. The build covered the .NET 9/10/11 library targets with their selected Aspire versions; tests run on .NET 11. This records local execution, not a new remote CI result. The passwordless Redis integration scenario also caught the stricter required-parameter behavior in Aspire 17: on .NET 10/11 its deferred secret parameter is optional, and the reference expression preserves empty-string environment resolution.

The optional package/API check also passed for Cloud Storage, MongoDB, Redis, RabbitMQ, Spanner and WireMock against published 12.0.0. The locally packed consumer ran successfully on .NET 9.0.23 with Aspire 13.6.5, .NET 10.0.15 with Aspire 17.0.0 and .NET 11.0.0 with Aspire 17.0.0.

Local tests establish the behavior of these helpers against the selected images. They do not establish equivalence with managed cloud services. In particular, successful local messaging does not prove production IAM or dead-letter policy behavior. The Apigee HTTP collection records observed cache behavior rather than promising a cache HIT.

Keep ordinary checks in the existing Cake pipeline. Package API compatibility remains an optional local check through [verify-compatibility.ps1](../eng/verify-compatibility.ps1); it complements behavioral tests. There is no new compatibility CI job or automatic cloud test requirement.

For a cloud-dependent contract, record the expected behavior and validate it in a dedicated authorized environment with cleanup, credentials and cost controls before relying on it in production. External adoption is a separate evidence question and does not change these runtime contracts.
