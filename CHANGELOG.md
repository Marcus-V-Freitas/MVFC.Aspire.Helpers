# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
