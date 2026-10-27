# Production runtime credentials

The existing application registration retains its `common` authority and
personal Microsoft plus organisational account audience. The server uses a
certificate for the confidential web client, never a new client secret.

`Runtime Infrastructure` plans the complete Production Terraform configuration
on infrastructure pull requests. Its input-free manual dispatch applies only
approved `main`; it never publishes application code or creates Development.
Review the full plan before dispatch. Retain legacy resources until the server,
authentication and isolated restores have passed acceptance.

PR plans are redacted log output only: no binary plan/state artifact is
published. The shared setup still enforces the committed CLI/provider pins.
The approved-main apply creates and consumes its saved plan in one isolated
job, without uploading it for repository-wide download.

After the catalog's scoped delegation and the reviewed vault/host permissions
are applied, dispatch:

```powershell
gh workflow run bootstrap-runtime.yml --repo frasermolyneux/talkwithtiles --ref main
gh workflow run renew-origin-tls.yml --repo frasermolyneux/talkwithtiles --ref main
```

The first workflow discovers the exact registration and dedicated vault from
the approved runtime example and validation policy, checks ownership, tenant
and account audience, and creates a database password only if missing. It
generates an ephemeral RSA key, appends the public certificate, authenticates
it against Entra, and only then publishes a base64 passwordless PFX. Private
keys never enter Terraform state, Git, command-line values or artifacts.
Management-plane publication does not require opening the default-deny vault
to hosted runner addresses. The host reads through its exact-vault Arc role
and fixed egress allowlist.

Existing database passwords are never implicitly rotated. An unchanged
reviewed configuration creates no new secret version. Trusted Caddy
addresses are resolved and injected only by the host.

Credential and TLS workflows are manual-only during initial rollout. Enable
their weekly renewal schedules in a reviewed post-acceptance change.
Certificate checks renew 180-day Entra certificates within 30 days of expiry.
Unrelated public keys and unexpired previous certificates are retained; only
this workflow's expired keys are eligible for removal after seven days.
The independent origin workflow requests a 365-day Origin CA certificate for
`talkwithtiles.com` and `www.talkwithtiles.com` and renews within 30 days. It
does not modify DNS or routes. Its separate exact-zone
`CLOUDFLARE_ORIGIN_API_TOKEN` is supplied by the workload catalog.

Missing established credentials, unexpected ownership, disabled secrets,
expiring database passwords or changed
certificate bindings stop publication and require explicit recovery. A failed
bootstrap can leave an unused public certificate; retry without deleting
existing keys. Verify real organisational and personal-account sign-in and
sign-out on the new domain before removing the legacy client-secret credential
or retiring Azure compute and storage.
