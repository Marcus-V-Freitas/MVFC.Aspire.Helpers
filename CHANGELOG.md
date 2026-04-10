# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
  https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/commit/e02f73f30605268e4d4dad76c6406758e4468c50

### Documentation
- Updated documentation. (commit 4e8028f)  
  https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers/commit/4e8028f90735da5fd0c260af6ea119270920bd9b
  
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

