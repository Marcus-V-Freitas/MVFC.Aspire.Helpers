# Contributing to MVFC.Aspire.Helpers

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or later
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) running locally
- Git

## Running locally

```sh
git clone https://github.com/Marcus-V-Freitas/MVFC.Aspire.Helpers.git
cd MVFC.Aspire.Helpers
dotnet restore MVFC.Aspire.Helpers.slnx
dotnet build MVFC.Aspire.Helpers.slnx --configuration Release
```

## Running tests

The tests use `Aspire.Hosting.Testing` and require Docker to be running.

```sh
dotnet test test/MVFC.Aspire.Helpers.Tests/MVFC.Aspire.Helpers.Tests.csproj --configuration Release
```

## Adding a new helper

1. Create a new folder under `src/MVFC.Aspire.Helpers.{ServiceName}/`
2. Follow the structure of an existing helper (e.g. `MVFC.Aspire.Helpers.Redis`)
3. Add the new project to `MVFC.Aspire.Helpers.slnx`
4. Add the package version to `Directory.Packages.props`
5. Add integration tests in `test/MVFC.Aspire.Helpers.Tests/`
6. Update `README.md` and `README.pt-BR.md` with the new package entry

## Branch naming

- `feat/` — new feature or helper
- `fix/` — bug fix
- `chore/` — dependency update or maintenance
- `docs/` — documentation only
- `test/` — tests only
- `refactor/` — no feature change, no bug fix

Example: `feat/add-kafka-helper`

## Commit convention

This project follows [Conventional Commits](https://www.conventionalcommits.org/):

- `feat: add Kafka helper`
- `fix: fix MongoDB replica set initialization timeout`
- `docs: update README badges`
- `chore: bump Aspire.Hosting to 13.2.0`
- `test: add WireMock integration tests`
- `refactor: simplify Redis commander setup`

## Pull Request process

1. Fork and create your branch from `main`
2. Make your changes and ensure all tests pass locally
3. Open a PR against `main` and fill in the PR template
4. Wait for the CI to pass
5. A maintainer will review and merge
