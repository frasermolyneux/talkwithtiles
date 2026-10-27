# PostgreSQL container runtime

## Deployment boundary

Production is `https://talkwithtiles.com`, through Cloudflare and the host's
trusted Caddy proxy. Anonymous landing pages and authenticated game flows are
preserved. The host owns ingress, certificate/secret delivery, backups and
activation. `workload.json` is compile-only until **both** zero digest placeholders
are replaced by reviewed published digests; pulling a placeholder must fail.
There is no hosted Development or database host port in production.
Production also accepts `www.talkwithtiles.com` and redirects it with HTTP 308 to
the HTTPS apex, preserving path/query and method, before starting authentication.
The host must route and terminate TLS for both names.

| Service | UID:GID | Writable bind |
|---|---|---|
| `talkwithtiles-web` | `1654:1654` | `keyring` → `/app/state/keys` |
| `talkwithtiles-postgres` | `999:1654` | `postgres` → `/var/lib/postgresql/data` |

Both images declare exec health checks, non-root users and no `VOLUME` metadata.
The host runs them with read-only rootfs, dropped capabilities, no-new-privileges,
bounded CPU/memory/PIDs and `/tmp` tmpfs. Database networking is private/internal;
only web joins `public-proxy` (mapped by the host to `platform-ingress`). Workload
ingress is empty because public routing belongs to the host configuration.

## Runtime configuration

`TALKWITHTILES_RUNTIME_FILE` defaults to `/run/secrets/runtime-configuration`.
JSON loads before service registration; environment variables can override it.
See the exact non-secret schema in
[`deployment/runtime.example.json`](../deployment/runtime.example.json).

- `Database:Host/Port/Name/Username/PasswordFile`: defaults use database/user
  `talkwithtiles`; host is `talkwithtiles-postgres` in production.
- `AzureAd:Instance/TenantId/ClientId/CallbackPath/SignedOutCallbackPath/ClientCertificateFile`:
  existing app `72ea5049-afe4-452a-92cc-a2c204a049e4`, tenant `common`,
  `AzureADandPersonalMicrosoftAccount` registration. Both organisational Entra and
  personal Microsoft accounts remain supported. Canonical callback is
  `https://talkwithtiles.com/signin-oidc`.
  Authorization codes are redeemed through MSAL using the certificate; the
  personal-account `offline_access` scope is retained. The token cache is
  in-process only and requests no additional downstream API permissions.
- `ForwardedHeaders:KnownProxies`: **explicit IP addresses of Caddy**, rendered by
  the host trusted-proxy adapter; Production fails without them. Only forwarded
  scheme/client IP are used, never forwarded Host. Unknown proxies are ignored.
- `DataProtection:KeyPath`: `/app/state/keys`; `AllowedHosts`:
  `talkwithtiles.com;www.talkwithtiles.com`.

Text secrets:

1. `runtime-configuration`: web-only JSON.
2. `client-certificate`: web-only **base64 passwordless PFX with private key**.
   The app loads it with ephemeral key storage; no runtime binary decoding or
   persistent private-key store is needed. The parent registers the public
   certificate and manages rotation. Do not supply a client secret.
3. `database-password`: web and PostgreSQL; password is read from the mounted
   file and never stored in JSON or the manifest. Rotate by recreating database
   then web with the same new mount; PostgreSQL reapplies the app-role password
   at startup. The bootstrap superuser has no TCP authentication rule; the app
   role is NOSUPERUSER/NOCREATEDB/NOCREATEROLE/NOREPLICATION.

Database connections use SCRAM on the dedicated private network (TLS and GSS
encryption negotiation are disabled; the database must never be published).
Disabling GSS also avoids Windows domain-discovery stalls in the local harness.

Recreate containers after secret/configuration changes. Do not put secret values
or private certificates in source control or CI artefacts.

The protected main-only credential and Origin TLS workflows check weekly on
Monday at 06:40 and 06:55 UTC respectively, with manual dispatch retained.
Certificates renew only within 30 days of expiry. Existing database passwords
and unrelated/unexpired Entra credentials are retained; missing established
credentials fail closed rather than resetting the runtime. These jobs do not
deploy images or retire Azure resources.

## Persistence, readiness and restart

Startup performs the versioned schema initialization in one transaction under a
database advisory lock; unsupported versions fail closed. A game (including
board coordinates, bag, racks and manager state) is a single JSONB snapshot.
Player indexes, newly created invites, consumed invites and contact updates are
committed in the same game transaction. Reads cannot observe partially replaced
tiles. Saves retain the engine's optimistic version: conflicting stale writers
return HTTP 409, requiring reload; identical aggregate retries are no-ops.
Dashboard hiding remains scoped to a player and survives later game saves.
Contact creation is limited to newly formed player pairs; moves or unrelated
membership changes do not recreate a contact a player deleted. Shared contact
locks are acquired in consistent player-ID order across games.
Contact/invite mutations and deletions are scoped and retry-safe. Invite
acceptance requires the current email's persisted invitation, not a display name.

`/api/health/live` proves the process; `/api/health/ready` verifies the PostgreSQL
writer and expected schema. Startup fails if persistence is unavailable. The
image healthcheck calls readiness and fails on database loss. The PostgreSQL
image initializes the bind directory as UID999 without inherited anonymous
volumes. Its temporary bootstrap server is Unix-socket-only; the TCP healthcheck
cannot pass before role/database initialization and the final network listener.
The image restores PostgreSQL's SIGINT fast-shutdown signal so a stop closes
pooled client sessions cleanly rather than waiting until Docker forcibly kills it.

In-memory sessions intentionally reset on web restart (display preferences,
alerts and other ephemeral UI state); there is no Redis. Games, contacts and
invitations persist in PostgreSQL. Auth cookies remain decryptable only when the
same durable Data Protection keyring and application name are retained. The host
must restrict and back up the unencrypted-at-rest keyring; lost keys require
users to sign in again.

The initial cutover is **fresh state**, with no Azure Table data transfer. To
restore later, stop/fence web writes, restore a verified PostgreSQL 18 logical
backup into the dedicated database or restore its stopped, consistent data
directory, retain the corresponding keyring, then start database and web and
verify readiness and a persisted game. Never downgrade a data directory across
PostgreSQL major versions. App rollback is safe only while schema versions are
compatible; host rollback is not a database rollback.

## Disposable local/CI database

Generate test-only credentials in an ignored project directory (never use the
production vault or a live database). For example in PowerShell:

```powershell
New-Item -ItemType Directory -Force .local | Out-Null
$env:TEST_POSTGRES_PASSWORD = [guid]::NewGuid().ToString('N')
$env:TEST_POSTGRES_PASSWORD_FILE = "$PWD\.local\database-password"
Set-Content -Path $env:TEST_POSTGRES_PASSWORD_FILE -Value $env:TEST_POSTGRES_PASSWORD -NoNewline
docker run --detach --name talkwithtiles-local-postgres `
  --publish 127.0.0.1:5432:5432 `
  --env POSTGRES_DB=talkwithtiles --env POSTGRES_USER=talkwithtiles `
  --env "POSTGRES_PASSWORD=$env:TEST_POSTGRES_PASSWORD" postgres:18-bookworm
dotnet test src\MX.TalkWithTiles.slnx
npm ci
npx playwright install chromium
npx playwright test --project=chromium
```

Playwright starts the app only, waits for database readiness and truncates the
**disposable test database** before the suite. Never point these test variables
at production. Keep one worker for deterministic shared multiplayer state.
For interactive local testing set `ASPNETCORE_ENVIRONMENT=Development`,
`Testing__Enabled=true`, `Database__PasswordFile` to the absolute test file path,
and `DataProtection__KeyPath` to an ignored local directory. Test sign-in requires
both explicit opt-in **and** Development; it remains disabled in Production even
when `Testing:Enabled=true`.

Cleanup only the named local test container with `docker rm --force
talkwithtiles-local-postgres` and remove `.local` when finished. CI runs real
PostgreSQL tests plus non-root/read-only image initialization, data survival
across recreation and failed readiness after database loss. A Docker daemon is
required for image tests; they are not simulated when it is unavailable locally.
