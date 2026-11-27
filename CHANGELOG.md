# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [12.3.0] - 2026-11-27

### Added

- Added opt-in `consumer.WaitForDumps(mongo)` to require a writable primary and successful dump completion before consumer startup. Concurrent consumers share one execution; canceling one wait does not cancel shared work started by that wait. Failures/cancellation are retained without automatically replaying partial writes.
- Added regression tests for seed ordering, shared execution, failure/cancellation and image overrides. Documented readiness levels, emulator fidelity and the Aspire 17/native MongoDB migration assessment in English and Portuguese.

### Changed

- Pinned the default Cloud Storage, BigQuery, Firestore, Pub/Sub/UI, Spanner, Mailpit, MongoDB, Redis and Redis Commander images to the digests used by the passing integration baseline. Existing image/tag/digest override methods remain available; custom Commander arguments are preserved.
- The playground now waits for MongoDB dumps before starting the API and uses package-provided Pub/Sub image digests.

### Upgrading from 12.2.1

- Public signatures and supported frameworks remain available. `WithReference(mongo)` keeps its consumer-ready lifecycle; add `.WaitForDumps(mongo)` when startup requires populated data. The new wait includes primary readiness and does not require a separate `.WaitFor(mongo)`.
- Default image annotations now select SHA256 instead of a floating tag. Use `WithDockerImage(image, tag)` or Aspire image overrides to select another version. Validate persisted data before changing server versions; reverting the library does not revert data formats.
- Aspire remains at 13.6.5 to preserve `net9.0` support. Aspire 17 requires a separately validated platform migration; the optional package compatibility check remains outside CI/CD.

## [12.2.1] - 2026-11-26

### Added

- Added httpyac scenarios for Apigee proxy routing, Basic Auth, JSON/XML transformations, CORS, method rejection, cache observations, aggregated health and shared flows, plus an opt-in six-request quota loop verifying HTTP 429.
- Added an isolated Cloud Storage CRUD scenario covering bucket creation, encoded object names, upload, metadata, prefix listing, download, overwrite, metadata updates, copy, direct deletion, the playground download/delete route and bucket cleanup. Expanded shared HTTP variables and English/Portuguese guides; the original destructive Storage scenario remains disabled by default.

### Upgrading from 12.2.0

- Public API signatures and playground routes are preserved. Invalid Pub/Sub identifiers, push requirements and subscription policy ranges now fail before endpoint resolution or subscription RPCs. Conflicting declarations for the same project/subscription are rejected; equivalent declarations are configured once.

### Fixed

- Prevented functional HTTP clients from retrying legacy GET routes that publish, consume, write or delete data after transient failures or lost responses. Read-only requests retain transient retries and named Aspire probes remain isolated.
- Validated all Pub/Sub declarations before subscription updates, including acknowledgment deadlines and DLQ delivery attempt ranges, preserving explicit zero service defaults. Coalesced equivalent subscription declarations and rejected conflicting topics, policies or push destinations before RPCs.
- Aligned whitespace-only push routes with pull behavior when building subscription update masks. Added regression coverage and updated English/Portuguese documentation.
- Expanded regression coverage for Spanner endpoint readiness, MongoDB readiness without dumps, replacement of Keycloak seeds with an empty collection, and WireMock form decoding and default DELETE response types.
- Separated platform detection from Apigee runtime argument configuration and Pub/Sub Docker host selection through internal methods, preserving public signatures and existing platform behavior. Tests now exercise Linux and non-Linux decisions on every host, alongside real-platform entry-point tests.
- Added regressions for Apigee readiness failures, cancellation before deployment and exhausted retries with the last failure in timeout diagnostics. The existing polling routine is internally accessible for deterministic retry-budget tests.
- Added Pub/Sub SDK tests for emulator environment selection, RPC cancellation and persisted subscription updates against a real emulator. Environment-changing tests run in an isolated nonparallel collection and restore the previous value.

## [12.2.0] - 2026-11-25

### Upgrading from 12.1.0

- Existing package API signatures and default startup behavior are preserved. Opt into stronger startup checks with `WithPrimaryHealthCheck()` for MongoDB and `WithTcpHealthCheck()` for Redis, then use `WaitFor(resource)` explicitly. Redis TCP checks do not validate authentication, and MongoDB primary readiness does not imply dump completion.
- Pub/Sub configuration still defaults to 60 seconds; use `WithConfigurationTimeout(seconds)` to override it per emulator. `WithWaitTimeout(seconds)` continues to control only image initialization.
- The playground now waits for API HTTP readiness, uses bounded Redis/RabbitMQ startup connections, and no longer requires the Pub/Sub UI to start the API. Functional HTTP tests no longer retry unsafe methods. Use `Infrastructure__StartupTimeoutSeconds` to override the default 30-second budget per infrastructure client.

### Added

- Added opt-in MongoDB writable-primary and Redis TCP health checks with three-second probe deadlines, idempotent registration and explicit `WaitFor` semantics. Redis hosting remains independent of the Redis SDK.
- Added per-emulator `WithConfigurationTimeout(seconds)`, preserving the shared 60-second default and separating subscription configuration from image initialization.
- Added a playground API readiness endpoint and configurable Redis/RabbitMQ startup budgets through `Infrastructure__StartupTimeoutSeconds` (30 seconds per client by default).

### Fixed

- Enabled compiler-generated source files on disk under `obj` so ReportGenerator can display generated logging code without missing-source warnings.
- Excluded test and playground assemblies explicitly from the Cake coverage report, preventing OpenAPI-generated types in the test assembly from lowering package coverage totals regardless of their namespace.
- Isolated functional HTTP test retries from named Aspire health probes, restored standard request deadlines, restricted retries to transient failures of safe methods, and preserved observable HTTP 429 responses. The shared fixture now waits for API readiness before functional requests.
- Removed the playground API startup dependency on the Pub/Sub administrative UI and replaced the RabbitMQ fixed startup delay with bounded connection retries. Redis requires an authenticated PING and disposes failed startup connections.

### Changed

- Pinned playground Pub/Sub emulator and UI images by digest without changing package image defaults. Added readiness, cancellation, timeout isolation and HTTP retry regression tests, and documented startup contracts in English and Portuguese.
- Completed package fluent-method references, startup examples and HTTP scenario readiness instructions in English and Portuguese; documented explicit assembly exclusions in Cake coverage reports.

## [12.1.0] - 2026-11-25

### Added

- Added 12 httpyac playground scenarios with 40 HTTP requests, JavaScript assertions and chained requests. Shared development variables are imported from `playground/http/variables.http`; scenarios run directly in the VS Code extension or CLI. RabbitMQ uses a temporary isolated queue, and the destructive Storage scenario is disabled by default.
- Added opt-in `MongoUpsertDump<T>` for full-document upserts by stable serialized `_id`, with batch identity validation and recovery tests after partial failure. The existing append dump remains unchanged.
- Added per-resource secret Redis password configuration through project references, preserving passwords containing commas and passwordless connections.
- Added optional local package API validation against published 12.0.0 packages and a package consumer harness for .NET 9, 10 and 11. This verification is not part of CI/CD.

### Fixed

- Infrastructure resilience integration tests now wait for a writable MongoDB primary and an authenticated Redis PING before exercising data operations, under the existing startup deadline. This addresses connection races observed on cold CI runners without changing production connection settings or global fixtures.
- MongoDB client caching now distinguishes case-sensitive connection strings, including passwords and database names.
- WireMock publishes its state before readiness and does not signal readiness for stopped servers.
- Spanner forwards cancellation to operation polling, observes final operation failures and distinguishes provisioning deadline expiration from AppHost cancellation.
- RabbitMQ connection references now include URI-escaped credentials, resolved after fluent configuration. The playground no longer duplicates RabbitMQ or Redis credentials.
- Repeated Redis password configuration produces a single server argument using the final secret.

### Changed

- Expanded behavioral tests for lifecycle ordering, cancellation, credential parsing, real authenticated connections, repeated seeds and partial-write recovery. Documented the consumer configuration contracts in English and Portuguese.

## [12.0.0] - 2026-11-24

### Breaking changes

- Push subscriptions now require `MessageConfig.PushEndpointReference` alongside their `PushEndpoint` route. `WithReference(pubSub)` no longer selects a push destination. To migrate, create the consumer project before the message configuration and set `PushEndpointReference = consumer.GetEndpoint("http")` on each push message. Pull subscriptions are unchanged. The single push destination was removed from the emulator resource.

### Added

- Added `MessageConfig.PushEndpointReference` for independent push destinations per subscription. Destination ports are resolved during subscription configuration under the shared deadline; each push subscription must explicitly specify its consumer endpoint. Added regression tests for multiple destinations, pull subscriptions, endpoint allocation and cancellation.
- Added a shared 60-second deadline for Pub/Sub subscription configuration, including endpoint resolution, client creation, SDK retries and cancellation support. Subscription reads now use bounded exponential backoff with jitter for transient errors and subscriptions still being created by the emulator image; updates retain the SDK retry policy.
- Added structured Pub/Sub initialization logs in the Aspire dashboard, including subscription and project details on failure, completion duration, and timeout diagnostics.
- Added Pub/Sub resilience tests covering transient recovery, permanent failures, retry exhaustion, cancellation, endpoint resolution timeout, repeated configuration after partial failure, configuration ordering, and structured logging. Added Redis Commander tests for password configuration before and after registration and for passwordless use. Expanded Firestore TCP health check tests for reachable ports, allocated and unallocated endpoints, missing configuration, and cancellation.

### Changed

- Reused the Redis password environment constant, centralized Keycloak HTTP port and import strategy identifiers and the Apigee backend URL environment variable, completed XML summaries on affected members, and standardized spacing between test methods.
- Separated runtime Pub/Sub initialization, deadline handling and per-subscription configuration; moved callback annotations into individual files without changing their namespaces.
- Standardized Arrange / Act / Assert sections and spacing in the affected tests, and centralized their imports and Pub/Sub imports in each project's `Usings.cs`.

### Fixed

- Fixed Pub/Sub push subscriptions targeting the emulator's port instead of the configured consumer endpoint's allocated port. Added regression tests for consumer endpoint selection and missing push destinations.
- Registered Pub/Sub and Spanner configuration callbacks when configurations are supplied, including configurations added after resource references, while preventing duplicate callback registration.
- Resolved Pub/Sub and Spanner emulator endpoints explicitly for configuration clients instead of changing process-wide emulator environment variables. Pub/Sub configuration now runs from the emulator's readiness callback.
- Deferred Firestore project ID environment resolution until execution so configurations added after references are included.
- Made Keycloak seed callbacks read the latest configured seeds and prevented duplicate callback registration.
- Deferred Pub/Sub consumer/UI project IDs and Redis Commander password resolution until execution, including configuration added after references or Commander registration.

### Documentation

- Corrected root and package documentation for explicit startup dependencies, the dotnet-coverage workflow, image-driven Pub/Sub provisioning, and the currently unused legacy `secondsDelay`/`UpDelay` values; removed duplicate Firestore documentation.
- Updated package READMEs in English and Portuguese with configuration ordering and initialization behavior, including Pub/Sub retry, timeout, cancellation, and logging semantics.

## [11.0.2] - 2026-11-21

### Fixed

- Fixed MongoDB dump execution when `WithReference` is invoked before `WithDumps`. Dump execution callback now resolves `mongo.Resource.Dumps` dynamically during the `OnResourceReady` event instead of discarding registration early when dumps are configured after references.
- Fixed MSBuild `MSB3021` file copy error targeting `*Undefined*\.editorconfig` by defining a fallback `SolutionDir` in `Directory.Build.props` when projects are built independently.

### Documentation

- Updated root and package READMEs (English and Portuguese) to document that `WithDumps` can be called before or after `WithReference`, detailing execution lifecycle behavior across multiple consumer projects.

## [11.0.1] - 2026-11-20

### Documentation

- Updated target framework requirements across root and package READMEs to .NET 9, 10 or 11 (or ou in Portuguese).
- Updated minimum Aspire.Hosting requirement to >= 13.6.5 across all READMEs.
- Updated Google.Cloud.PubSub.V1 minimum version to >= 3.39.0 in PubSub documentation.
- Documented MVFC.Aspire.Helpers.Playground.AppHost and unit/integration test structure breakdown in README.md and README.pt-BR.md.
- Added **Architecture Overview** diagram in Mermaid illustrating the interaction between Playground, helper packages, and internal mechanics.
- Added **Internal Mechanics** architectural reference detailing OnResourceReady lifecycle hooks, TCP/gRPC health checks, temporary seed/bundle generation, environment variable injection via WithReference, and duplicate prevention annotations.
- Added **Dependencies** section mapping centrally managed package versions from Directory.Packages.props.
- Added **Testing** section describing test separation (unit vs. AppHost integration, xUnit v3, Microsoft.Testing.Platform, Coverlet).
- Added **Publishing** section describing CI automated publishing via MinVer and * tags.
- Clarified development environment requirements differentiating global.json SDK pinning (10.0.100) from library runtime support.

## [11.0.0] - 2026-11-12

### Added

- Added support for `.NET 11` (`net11.0`) across all Aspire helper packages, playground applications, and test suites.
- Added framework-conditional package references for `net11.0` in `Directory.Packages.props`, including `Microsoft.OpenApi 3.11.0`.
- Added .NET 11 setup in CI/CD build and publish workflows.

### Changed

- Updated NuGet package dependencies (`Aspire.Hosting` to `13.6.5`, `MongoDB.Driver` to `3.14.0`, `Scalar.AspNetCore` to `2.21.0`, `StackExchange.Redis` to `3.4.35`, `WireMock.Net.minimal` to `2.31.0`, and test packages).
- Configured centralized version properties (`AspireVersion`, `Net11Version`, `Net10Version`, `Net9Version`) in `Directory.Packages.props`.
- Updated `global.json` SDK configuration to allow pre-releases and roll forward to `latestMajor`.
- Updated platform documentation badges across all README files to reflect `.NET 9 | 10 | 11` support.

## [10.1.7] - 2026-11-07

### Changed

- Restricted CI test and coverage execution to internal pull requests.
- Re-enabled release tag validation in the publish pipeline using explicit `origin/main` refspec fetching.

## [10.1.6] - 2026-11-07

### Changed

- Simplified CI/CD publish pipeline by removing the release tag ancestry validation check against `main`.

## [10.1.5] - 2026-11-07

### Changed

- Optimized CI workflow to skip redundant test and coverage execution on `main` when commits originate from merged pull requests.

## [10.1.4] - 2026-11-07

### Changed

- Updated NuGet package dependencies (`Scalar.AspNetCore` to `2.19.0`, `StackExchange.Redis` to `3.4.31`, and `WireMock.Net.minimal` to `2.30.0`).
- Updated CI workflow to run tests on push to `main` and refined release publishing conditions.
- Standardized documentation tables and updated CI badge branch targeting across READMEs.

## [10.1.3] - 2026-11-01

### Added

- Added root `.editorconfig` defining centralized coding standards, naming conventions, and Roslyn/Sonar analyzer settings.
- Added `dotnet-coverage` tool (`18.12.0`) to local .NET tool manifest.
- Configured SDK version `10.0.100` with `latestFeature` roll-forward strategy in `global.json`.

### Changed

- Updated CI/CD workflow with concurrency control, validation of release tag ancestry on `main`, and pinned tool execution (`dotnet tool restore` and `dotnet tool run`).
- Updated `MVFC.Pack.Analyzers` package reference to `4.0.2`.
- Suppressed analyzer rules `S1313` (hardcoded IP addresses) and `S5332` (HTTP/SSL in local emulator endpoints) with `severity = none` across scoped and root `.editorconfig` files.

### Fixed

- Fixed parameter ordering in `WithHttpHealthCheck` within `ApigeeEmulatorExtensions`.
- Propagated `CancellationToken` to `GetSubscriptionAsync` call in `PubSubConfigurator`.

## [10.1.2] - 2026-10-31

### Changed

- Updated CI/CD workflow to authenticate NuGet package publishing via GitHub Actions OIDC trusted publishing (`NuGet/login@v1`).
- Enhanced tag and release cleanup on pipeline failure with explicit fallback handling.

## [10.1.1] - 2026-10-31

### Changed

- Updated NuGet package dependencies to address security vulnerabilities: `Aspire.Hosting`, `Aspire.Hosting.AppHost`, `Aspire.Hosting.Testing` to `13.6.4`, `Microsoft.AspNetCore.Authentication.JwtBearer` and `Microsoft.Extensions.Diagnostics.HealthChecks` to `10.0.14`, `Microsoft.OpenApi` to `2.12.2`, and `Microsoft.NET.Test.Sdk` to `18.9.0`.
- Added `Microsoft.AspNetCore.OpenApi` package reference (`10.0.14`) to the playground API to enable OpenAPI document generation required by Scalar UI.

## [10.1.0] - 2026-08-30

### Added

- Created Google Cloud BigQuery emulator helper integration (`AddGcpBigQuery`) with configurations for projects, datasets, and YAML data seeding.
- Added comprehensive unit tests and AppHost integration tests for `GcpBigQuery`.
- Integrated BigQuery into the playground API and AppHost with working data dumps.
- Added structured documentation for the new BigQuery module (`README.md` and `README.pt-BR.md`) aligned with standard project formatting.

### Changed

- Updated NuGet package dependencies listed in documentation across all helpers to match `Directory.Packages.props` (e.g., `Aspire.Hosting` to `13.5.3`).
- Standardized documentation to explicitly require "Docker running locally" and updated the `.csproj` standardizations.
- Added missing "Requirements" sections to Keycloak documentation.

## [10.0.0] - 2026-08-30

### Added

- Added `EndpointReference` support to `SpannerGrpcHealthCheck` and `FirestoreTcpHealthCheck`, allowing dynamic resolution of host and port at runtime for resources with dynamically allocated ports.
- Added `<AspireUseCliBundle>true</AspireUseCliBundle>` property to `MVFC.Aspire.Helpers.Playground.AppHost.csproj`.
- Added `CustomInstantiator` in `MongoFaker.cs` for reliable instantiation of `TestDatabase` seed models.

### Changed

- Configured `isProxied: true` across all helper container endpoints (`ApigeeEmulator`, `CloudStorage`, `GcpFirestore`, `GcpPubSub`, `GcpSpanner`, `Gotenberg`, `Keycloak`, `Mailpit`, `Mongo`, `RabbitMQ`, and `Redis`) to route traffic through Aspire reverse proxy.
- Updated `GcpFirestore` endpoint registration to use Aspire's native `.WithHttpHealthCheck("/")`.
- Refactored `PubSubEmulatorExtensions` to dynamically resolve `PUBSUB_EMULATOR_HOST` from `ConnectionStringExpression` on resource ready.
- Refactored `PubSubConfigurator` to instantiate `SubscriberServiceApiClient` asynchronously on demand, accept explicit client instances in overloads, and use `CultureInfo.InvariantCulture` for port formatting.
- Updated `MVFC.Aspire.Helpers.Playground.Api` and `MVFC.Aspire.Helpers.Playground.AppHost` launch profiles (`launchSettings.json`) to bind `0.0.0.0` for container and external accessibility.
- Updated `Microsoft.NET.Test.Sdk` package reference to `18.3.0`.

### Removed

- Removed manual `RegisterTcpHealthCheck` helper method from `FirestoreEmulatorExtensions` in favor of native Aspire HTTP health checking.

## [9.0.5] - 2026-08-27

### Added

- Added `global.json` configuring `Microsoft.Testing.Platform` test runner.
- Added `Microsoft.Testing.Platform` package reference to tests project.
- Added missing package references (`Microsoft.OpenApi` in Playground API and `Scriban.Signed` in Playground AppHost).

### Changed

- Upgraded test suite dependencies (`xunit.v3` and `xunit.runner.visualstudio`) to `4.0.0`.
- Adjusted test coverage arguments in `build.cake` for compatibility with `Microsoft.Testing.Platform`.
- Updated NuGet package dependencies (`Google.Cloud.PubSub.V1`, `Google.Cloud.Firestore`, `Microsoft.NET.Test.Sdk`, `MongoDB.Driver`, `NSubstitute`, `RabbitMQ.Client`, `Scalar.AspNetCore`, `StackExchange.Redis`, `WireMock.Net.minimal`, and ASP.NET Core packages).

## [9.0.4] - 2026-06-26

### Changed

- Updated NuGet package dependencies.

## [9.0.3] - 2026-04-12

### Changed

- Removed leftover `FirestoreConfiguredAnnotation` class and cleaned up unused global `using` directives across the `GcpFirestore` and tests projects.
- Refactored `WithReference` documentation comment by removing outdated references to `OnResourceReady`.

## [9.0.2] - 2026-04-12

### Changed

- Removed `WithWaitTimeout` and emulator readiness polling logic in `GcpFirestore`, shifting entirely to relying on native Aspire expression evaluation for injecting connection strings.
- Removed unused resources like `RegisterFirestoreConfigurator` and test cases verifying manual wait timeout boundaries.
- Updated documentation to reflect the removal of `WithWaitTimeout` from public methods and examples.

## [9.0.1] - 2026-04-12

### Changed

- Changed default external port for Firestore emulator from `8080` to `8084` to prevent collision with Apigee emulator.
- Refactored `GcpFirestore` integration by removing manual `secondsDelay` in favor of dynamic emulator readiness checking (fetching the connection string directly from the emulator expression).
- Updated `FirestoreConfig` to only require `projectId`.
- Removed `FirestoreConfigurator` class and associated tests.
- Updated documentation (`README.md` and `README.pt-BR.md`) to reflect the simplified configuration.

## [9.0.0] - 2026-04-11

### Added

- Created Google Cloud Firestore emulator helper integration (`AddGcpFirestore`) with configuration methods and readiness validation.
- Added comprehensive unit tests for `GcpFirestore`, covering configurations, extensions, and health checks.
- Integrated Firestore into the playground API and AppHost.
- Documented the new Firestore module with structured `README.md` and `README.pt-BR.md` aligned with standard project formatting.

## [8.0.2] - 2026-04-10

### Added

- Added scoped `.editorconfig` files to `src/` and `playground/` directories to cleanly suppress project-level analyzer rules (`CA1707`, `MA0002`, `MA0048`, `S2068`).

### Changed

- Refactored `ApigeeEndpoints` in the playground API by breaking down endpoint mappings into smaller, focused methods for better maintainability.
- Enforced strict null validation in `PubSubProjectBuilder` by explicitly throwing `ArgumentNullException`, updating corresponding unit tests.
- Applied `.ConfigureAwait(false)` in `ApiHelper` asynchronous calls to adhere to async best practices.

## [8.0.1] - 2026-04-02

### Added

- Expanded unit test coverage for **RabbitMQ**, **GcpSpanner**, **Mongo**, **ApigeeEmulator**, and **WireMock**, specifically targeting asynchronous resource readiness callbacks and boundary logic.
- Created `WireMockLifecycleHookTests` to verify and validate resource lifecycle states (`Running`, `Error`) and generated log messages.
- Added comprehensive property and hashing tests for `RabbitMQDefinitionsBuilder` to ensure reliable generation of RabbitMQ configuration files.

### Changed

- Improved `WireMockLifecycleHookTests` robustness by implementing dynamic port allocation to prevent conflicts during concurrent test runs.
- Standardized `AuthenticationHeaderValue` usage across unit tests for consistency and better namespace handling.

## [8.0.0] - 2026-04-02

### Added

- Integrated **MinVer** for automatic Semantic Versioning based on Git tags.
- Configured `MinVerTagPrefix` as `v` to align with existing project tagging conventions.

### Changed

- Simplified GitHub Actions CI workflow by removing manual version extraction and project patching scripts.
- Optimized MSBuild property functions in `Directory.Build.Props` for more robust project categorization and exclusion (e.g., playground projects).

## [7.3.3] - 2026-03-31

### Added

- Added a featured mention section in the documentation (`README.md` and `README.pt-BR.md`) highlighting the MVFC.Aspire.Helpers article on the GFT Technologies Engineering Blog.

## [7.3.2] - 2026-03-29

### Changed

- Removed the pending section from the Apigee Emulator documentation (`README.md` and `README.pt-BR.md`).

## [7.3.1] - 2026-03-29

### Added

- Added comprehensive unit tests for `WithBackend` validation, covering duplicate target server name prevention and parameter validation.
- Added new test helpers (`ApigeeEmulatorTestHelpers`, `FakeBackend`) to streamline and simplify emulator unit testing.

### Changed

- Extracted target server duplication validation into a dedicated method (`ThrowIfTargetServerAlreadyRegistered`) in `ApigeeEmulatorExtensions.cs`.
- Enhanced `ApigeeEmulatorLifecycleHook.cs` to ensure the proxy bundle `.zip` is reliably deleted after deployment attempts using a `try...finally` block.
- Significantly expanded Apigee Emulator documentation (`README.md` and `README.pt-BR.md`), detailing runtime behavior, troubleshooting steps, and comprehensive policy explanations.

## [7.3.0] - 2026-03-29

### Added

- Added support for multiple backend TargetServers in the Apigee Emulator lifecycle hook, dynamically mapping multiple Aspire backend resources.
- Added a new `/sharedflow-check` validation endpoint in the playground API and `demo-api` proxy to verify SharedFlow integrations.
- Added comprehensive unit and integration tests covering multiple TargetServers generation and the new SharedFlow endpoint.

### Changed

- Refactored the `common-logging` SharedFlow by replacing the deprecated `JS-LogRequestResponse` policy with an `AssignMessage` policy (`AM-InjectLoggingHeaders`) to inject correlation IDs and metadata natively.
- Updated flow diagrams and policy applicability tables in the Apigee Emulator READMEs to reflect the new `/sharedflow-check` interaction.

## [7.2.2] - 2026-03-29

### Changed

- Changed `HealthCheckPath` accessibility in `ApigeeEmulatorResource` from `public` to `internal`.
- Updated flow diagrams and policy applicability documentation in Apigee Emulator READMEs.

## [7.2.1] - 2026-03-29

### Changed

- Minor Apigee improvements: fixed documentation typo for control port, added bundle zip cleanup post-deployment, and added strict JSON validation for target servers.
- Improved unit test robustness in `ApigeeEmulatorLifecycleHookTests` by replacing `Arg.Any` catch-all matchers with exact parameter validations (such as verifying strict file/directory paths and zip names).

## [7.2.0] - 2026-03-29

### Added

- Created Google Apigee Emulator helper integration (`AddApigeeEmulator`) with extensions for workspace configuration, bundle deployment, and dynamic TargetServer wiring.
- Added a comprehensive `demo-api` workspace equipped with complete proxy policies and flows.
- Added unit and integration tests for the Apigee Emulator helper.
- Integrated the Apigee Emulator into the Playground with relevant example endpoints.

### Changed

- Adjusted Keycloak ports and overall playground configuration.
- Updated the solution structure to accommodate the new Apigee module and tests.

## [7.1.0] - 2026-03-22

### Changed

- Reduced package icon sizes and included the missing GCP Spanner icon.

## [7.0.0] - 2026-03-22

### Added

- Created GCP Spanner helper integration (`AddGcpSpanner`) with health check and emulator extensions.
- Added comprehensive unit tests for `GcpPubSub`, `Mongo`, and `GcpSpanner`, increasing overall line coverage to 97.6%.
- Added Mermaid sequence diagrams for provisioning flow to all helper project READMEs.

## [6.4.4] - 2026-03-21

### Changed

- CI/CD workflow refinements for automated publishing and coverage reporting
- Minor adjustments to Codecov configuration for status checks precision

## [6.4.3] - 2026-03-15

### Fixed

- Fixed badge links to point to the correct codecov.

## [6.4.2] - 2026-03-15

### Added

- Added unit tests for all integration modules.
- Improved ci pipeline to include better coverage report generation.
- Improved documentation with badges.

### Changed

- Renamed test folder to tests
- Updated coverage report generation to include all test projects

## [6.4.1] - 2026-03-10

### Added

- Simplified the playground example and simplified collection dump configuration. (commit e02f73f)  
  <https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/commit/e02f73f30605268e4d4dad76c6406758e4468c50>

### Documentation

- Updated documentation. (commit 4e8028f)  
  <https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/commit/4e8028f90735da5fd0c260af6ea119270920bd9b>
  
## [6.4.0] - 2026-03-08

### Added

- Comprehensive unit tests for all integration modules (`GcpPubSub`, `Gotenberg`, `Keycloak`, `Mailpit`, `Mongo`, `RabbitMQ`, and `Redis`).
- Reorganized integration tests into a dedicated `Integration/` folder for better separation of concerns.
- Added `AppHelper` and specialized stubs (`StubRealmSeed`, `NoOpMongoDump`) to enhance the testing infrastructure.

### Changed

- Improved overall code coverage, approaching 100% in critical components.
- Refined assembly meta-data and global usings across the test suite.

## [6.3.0] - 2026-03-08

### Added

- Added comprehensive examples for Mailpit, Redis, and RabbitMQ with credentials in the playground.
- Added persistent data volumes to multiple services in the playground (Keycloak, Mongo, RabbitMQ, Redis).
- Created English and Portuguese READMEs for Gotenberg integration.
- Added XML documentation for Keycloak configuration helpers.
- Updated main README with Gotenberg and unified usage examples.

## [6.2.0] - 2026-03-08

### Changed

- Refactored tests to use Refit for HTTP client abstraction
- Added Keycloak helper support

## [6.1.1] - 2026-03-07

### Fixed

- Fixed template file name

## [6.1.0] - 2026-03-07

### Added

- Added Gotenberg helper (PDF conversion)
- Reduced package icon sizes
- Added new test examples

### Changed

- Improved `.editorconfig`
- Updated documentation for all packages

## [6.0.0] - 2026-03-07

### Changed

- **Breaking:** Standardized method signatures to follow the Aspire builder pattern
- Internal improvements and documentation updates

## [5.1.0] - 2026-02-28

### Added

- Dynamic port support

### Changed

- Simplified internal structures and test setup

## [5.0.3] - 2026-02-17

### Changed

- Centralized package management (`Directory.Packages.props`)
- Improved async patterns in playground API

## [5.0.2] - 2026-02-17

### Added

- Scalar + OpenAPI support in playground API

### Changed

- Internal organization improvements

## [5.0.1] - 2026-02-17

### Added

- FluentAssertions to test project
- WireMock dispose fix
- Improved test timeout handling

## [5.0.0] - 2026-02-16

### Added

- RabbitMQ helper
- Redis + Redis Commander helper

## [4.3.0] - 2026-02-15

### Changed

- Improved separation of concerns in WireMock helper
- WireMock functionality improvements

## [4.2.0] - 2026-02-03

### Changed

- Optimized GCP Pub/Sub internals
- Added duplicate resource handling

## [4.1.0] - 2026-01-30

### Added

- Webhook example in playground
- Updated packages (xunit)

## [4.0.2] - 2025-11-01

### Changed

- Improved CloudStorage internal organization

## [4.0.1] - 2025-11-01

### Fixed

- Fixed Mailpit README

## [4.0.0] - 2025-11-01

### Added

- Mailpit helper (SMTP emulator)
- Apache-2.0 license applied to all packages

## [3.0.0] - 2025-10-30

### Added

- WireMock.Net helper

### Changed

- **Breaking:** Refactored library into separate domain packages (`MVFC.Aspire.Helpers.*`)

## [2.0.2] - 2025-10-30

### Changed

- Improved README documentation

## [2.0.1] - 2025-10-30

### Fixed

- Fixed package path in publish workflow

## [2.0.0] - 2025-10-30

### Changed

- **Breaking:** Refactored library into separate domain packages

## [1.0.6] - 2025-10-23

### Added

- MongoDB: allow creation of topics only
- MongoDB: fixed volume support in emulator

## [1.0.5] - 2025-10-23

### Added

- MongoDB dump support

## [1.0.4] - 2025-10-23

### Added

- Configurable wait timeout parameter

## [1.0.3] - 2025-10-22

### Added

- Dead-letter support for GCP Pub/Sub

## [1.0.2] - 2025-10-21

### Added

- Multi-project support

## [1.0.1] - 2025-10-21

### Added

- Internal topic creation support for GCP Pub/Sub

## [1.0.0] - 2025-10-19

### Added

- Initial release
- GCP Cloud Storage emulator helper
- GCP Pub/Sub emulator helper
- MongoDB Replica Set helper
- Cake build script
- NuGet publish workflow

[12.2.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v12.2.0...v12.2.1
[12.2.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v12.1.0...v12.2.0
[12.1.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v12.0.0...v12.1.0
[12.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v11.0.2...v12.0.0
[11.0.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v11.0.1...v11.0.2
[11.0.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v11.0.0...v11.0.1
[11.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v10.1.7...v11.0.0
[10.1.7]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v10.1.6...v10.1.7
[10.1.6]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v10.1.5...v10.1.6
[10.1.5]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v10.1.4...v10.1.5
[10.1.4]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v10.1.3...v10.1.4
[10.1.3]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v10.1.2...v10.1.3
[10.1.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v10.1.1...v10.1.2
[10.1.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v10.1.0...v10.1.1
[10.1.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v10.0.0...v10.1.0
[10.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v9.0.5...v10.0.0
[9.0.5]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v9.0.4...v9.0.5
[9.0.4]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v9.0.3...v9.0.4
[9.0.3]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v9.0.2...v9.0.3
[9.0.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v9.0.1...v9.0.2
[9.0.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v9.0.0...v9.0.1
[9.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v8.0.2...v9.0.0
[8.0.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v8.0.1...v8.0.2
[8.0.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v8.0.0...v8.0.1
[8.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v7.3.3...v8.0.0
[7.3.3]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v7.3.2...v7.3.3
[7.3.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v7.3.1...v7.3.2
[7.3.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v7.3.0...v7.3.1
[7.3.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v7.2.2...v7.3.0
[7.2.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v7.2.1...v7.2.2
[7.2.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v7.2.0...v7.2.1
[7.2.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v7.1.0...v7.2.0
[7.1.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v7.0.0...v7.1.0
[7.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.4.4...v7.0.0
[6.4.4]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.4.3...v6.4.4
[6.4.3]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.4.2...v6.4.3
[6.4.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.4.1...v6.4.2
[6.4.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.4.0...v6.4.1
[6.4.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.3.0...v6.4.0
[6.3.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.2.0...v6.3.0
[6.2.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.1.1...v6.2.0
[6.1.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.1.0...v6.1.1
[6.1.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v6.0.0...v6.1.0
[6.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v5.1.0...v6.0.0
[5.1.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v5.0.3...v5.1.0
[5.0.3]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v5.0.2...v5.0.3
[5.0.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v5.0.1...v5.0.2
[5.0.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v5.0.0...v5.0.1
[5.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v4.3.0...v5.0.0
[4.3.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v4.2.0...v4.3.0
[4.2.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v4.1.0...v4.2.0
[4.1.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v4.0.2...v4.1.0
[4.0.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v4.0.1...v4.0.2
[4.0.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v4.0.0...v4.0.1
[4.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v3.0.0...v4.0.0
[3.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v2.0.2...v3.0.0
[2.0.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v2.0.1...v2.0.2
[2.0.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v1.0.6...v2.0.0
[1.0.6]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v1.0.5...v1.0.6
[1.0.5]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v1.0.4...v1.0.5
[1.0.4]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v1.0.2...v1.0.3
[1.0.2]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v1.0.1...v1.0.2
[1.0.1]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/compare/v1.0.0...v1.0.1
[1.0.0]: https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/releases/tag/v1.0.0
