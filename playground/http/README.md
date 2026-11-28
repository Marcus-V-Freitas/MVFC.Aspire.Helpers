# Aspire playground HTTP scenarios

> 🇧🇷 [Leia em Português](README.pt-BR.md)

Collection for [httpyac](https://httpyac.github.io/guide/), with JavaScript scripts, assertions and request dependencies. Start the AppHost and wait for its resources to be ready before running it. These files do not change AppHost configuration, TTL settings, fixtures or CI/CD.

## Setup

The playground API now waits for a writable MongoDB primary and completed dumps through `WaitForDumps(mongo)` before starting. This guarantee belongs to the AppHost configuration; the HTTP scripts do not populate MongoDB themselves. Default emulator images use tested digests. See [runtime contracts](../../docs/runtime-contracts.md) for readiness levels and emulator limitations; local success does not establish cloud-service equivalence.

1. Start the playground from the repository root:

   ```powershell
   dotnet run --project playground/MVFC.Aspire.Helpers.Playground.AppHost
   ```

2. Copy the HTTP URL of `api-exemplo` from the Aspire dashboard. Its proxy port may differ from the internal port 5050.
3. Update the URLs in [variables.http](variables.http), automatically imported by all scenarios. Defaults are `5050` (API), `7070` (WireMock), `8081` (Keycloak), `15672` (RabbitMQ Management), `8998` (Apigee traffic, `apigeeUrl`) and `4443` (Cloud Storage, `storageUrl`). Check the endpoints published by the dashboard. Apigee requests use `/demo` on the traffic endpoint, rather than the control API on port `7071`.
4. Authentication values are public development seed data, named `brokerLocalAccount`, `brokerLocalValue`, `identityLocalClient`, `identityClientValue`, `identityLocalAccount`, `identityLocalValue`, `gatewayLocalAccount` and `gatewayLocalValue`. Do not put real credentials in this file. Neutral variable names do not change the parameters required by the protocols.

## VS Code

Before sending requests, confirm that `GET <apiUrl>/health/ready` returns HTTP 200. The API starts independently of the Pub/Sub administrative UI. Scenarios that call RabbitMQ Management or Keycloak directly also require those endpoints to be available. If an infrastructure client exceeds its startup budget, inspect the API startup error; configure `Infrastructure__StartupTimeoutSeconds` to override the default 30 seconds per Redis/RabbitMQ client. A ready API does not guarantee that all sample-data callbacks have completed.

With the **httpyac** extension installed, open a scenario `.http` file and select **Send All Requests**. No environment selection or `.env` files are needed: [variables.http](variables.http) provides defaults through `@import`. Each request region has a name for individual execution. Chained requests use `@ref`; to repeat an entire scenario, run **httpyac: Reset** first.

## Command line

Install the CLI once, if needed:

```powershell
npm install -g httpyac
```

Run from the repository root:

```powershell
# Connectivity: MongoDB, Firestore and Spanner
httpyac send playground/http/01-smoke.http --all --bail --output short

# Override a variable without editing files
httpyac send playground/http/02-redis.http --all --bail --var apiUrl=http://localhost:5050 --output short

# All scenarios, sequentially
httpyac send "playground/http/[0-9]*.http" --all --bail --output short --output-failed short

# One service
httpyac send playground/http/03-rabbitmq.http --all --bail --output short --output-failed short
```

The extension works without installing the CLI. Avoid running chained requests in parallel. Use `short` output for failures as well to avoid printing tokens or authentication headers.

## Scenarios and effects

| File | What it checks | Data effects |
| --- | --- | --- |
| [01-smoke.http](01-smoke.http) | MongoDB, Firestore and Spanner connectivity | Read-only |
| [02-redis.http](02-redis.http) | Missing key, write and exact value readback | Creates a `httpyac-<uuid>` key; the API has no delete/TTL route |
| [03-rabbitmq.http](03-rabbitmq.http) | Isolated queue/binding, API publication, exact message consumption and empty queue | Creates and removes only its `httpyac-<uuid>` queue; expires after 5 minutes of inactivity if execution stops |
| [04-firestore.http](04-firestore.http) | Document creation and lookup by returned identity | Retains the created user; the API has no DELETE route |
| [05-spanner.http](05-spanner.http) | User creation and lookup by unique name | Retains the created user; the API has no DELETE route |
| [06-bigquery.http](06-bigquery.http) | Dataset and seeded Alice/Bob users | Read-only |
| [07-pubsub.http](07-pubsub.http) | Real publication and simulated push receiver with a base64 envelope | Publishes a message and calls the receiver directly |
| [08-keycloak.http](08-keycloak.http) | Anonymous 401, token acquisition, authorized access and invalid credentials | Obtains a session/token for the development admin user |
| [09-gotenberg.http](09-gotenberg.http) | HTML conversion and `%PDF-` signature | Receives a PDF in memory |
| [10-mailpit.http](10-mailpit.http) | Sending through local SMTP | Adds an email captured by Mailpit; find its `httpyac-<uuid>` subject in the UI |
| [11-wiremock.http](11-wiremock.http) | Mock GET/POST/PUT/PATCH/DELETE, JSON, headers and authentication; API callback | Calls mocks and simulates a callback |
| [12-storage-opcional.http](12-storage-opcional.http) | Bucket download/delete route | **Deletes objects from the selected bucket; disabled by default** |
| [13-apigee.http](13-apigee.http) | Proxy routing, headers, Basic Auth, JSON transformation, XML conversion, CORS, blocked methods, cache observations, aggregated health and shared flow | Calls the proxy and populates a unique cache key |
| [14-storage.http](14-storage.http) | Bucket creation, upload, metadata, prefix listing, download, overwrite, metadata update, copy, direct deletion and API download/deletion | Creates and removes only its own `httpyac-<uuid>` bucket and objects |
| [15-apigee-quota-opcional.http](15-apigee-quota-opcional.http) | Loop: five accepted requests followed by HTTP 429 | Updates the quota counter for a unique identifier; disabled by default |

### Apigee

Wait until `GET <apigeeUrl>/demo/health` returns 200: API readiness alone does not guarantee that the proxy has been deployed. Run the normal and optional quota scenarios separately:

```powershell
httpyac send playground/http/13-apigee.http --all --bail --output short --output-failed short
httpyac send playground/http/15-apigee-quota-opcional.http --all --bail --var allowApigeeQuota=true --output short --output-failed short
```

The normal scenario checks invalid and valid Basic Auth using public demonstration values, without printing authorization headers. Its repeated cache request accepts `HIT` or `MISS` and compares the cached body when `HIT` is reported. The current local emulator returned `MISS` on both calls during validation, so the collection does not claim to prove cache reuse. This scenario does not run a SpikeArrest burst or call the external `/demo/status` target.

The quota scenario uses one unique `X-Forwarded-For` identifier across six sequential requests within the configured one-minute window. Do not pause the loop, run it in parallel or treat this demonstration header as a production authorization mechanism. Use **httpyac: Reset** before repeating the scenario in the extension.

### Cloud Storage CRUD

```powershell
httpyac send playground/http/14-storage.http --all --bail --output short --output-failed short
```

This scenario calls the emulator JSON API directly through `storageUrl`, then exercises the real playground download/delete route through `apiUrl`. It generates its own bucket name and URL-encodes object names containing folders and spaces. It does not use the configurable `storageBucket` or the seeded `bucket-teste`.

The final requests delete the temporary copy, let the playground API consume and delete the remaining object, verify the bucket is empty, delete the bucket and confirm HTTP 404. Deletion accepts HTTP 200 (observed in the local emulator) or 204, with follow-up reads proving removal. If `--bail` or an interruption stops the scenario, cleanup may not run: inspect the generated bucket name in the request URL and remove only that bucket using the emulator JSON API. No automatic expiration is configured.

### RabbitMQ

The scenario does not change or consume `test-queue`, `empty-queue` or `dlq`. It uses `test-exchange`, a `httpyac.<uuid>` routing key and an isolated queue without message TTL. This checks publication and consumption independently of the demonstration settings on `test-queue`. The Management API must be available, and the user must be allowed to create queues and bindings. `x-expires` controls queue expiration through inactivity, not message TTL. If an assertion fails, `--bail` stops execution and expiration takes care of the temporary queue.

### Pub/Sub and callbacks

The publication endpoint sends the fixed message implemented by the API. Calling the receiver directly checks its HTTP contract and envelope; it does not prove that the emulator delivered the push. The current receiver has no endpoint for querying received deliveries. These files do not claim to verify that end-to-end behavior.

### Optional Storage scenario

The playground GET route also deletes the objects it reads. It stays disabled even when running all scenarios. To exercise it, use a disposable bucket and enable it explicitly:

```powershell
httpyac send playground/http/12-storage-opcional.http --all --bail --var allowStorageDelete=true storageBucket=my-disposable-bucket --output short
```

To reset simulation data, remove your keys/users with the corresponding emulator tools; do not delete shared volumes or databases to run the collection.
