# Production runtime credentials

The existing application registration retains its `common` authority and
personal Microsoft plus organisational account audience. The server uses a
certificate for the confidential web client, never a new client secret.

`Runtime Infrastructure` plans the complete Production Terraform configuration
on infrastructure pull requests. Its input-free manual dispatch applies only
approved `main`; it never publishes application code or creates Development.
Review the full plan before dispatch. Legacy resource retirement requires
separate authorization and verified server, authentication and restore acceptance.

The Runtime Infrastructure PR plan runs without publishing a binary plan
artifact. PR Verify also has a separately configured opt-in Production plan;
inspect its workflow and artifact handling before using it. Terraform pins are
maintained in the root and workflow configuration.

After the scoped delegation and reviewed vault/host permissions are applied,
dispatch:

```powershell
gh workflow run bootstrap-runtime.yml --repo frasermolyneux/talkwithtiles --ref main
```

The workflow discovers the exact registration and dedicated vault from
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

The runtime credential workflow runs on the schedule in
[`bootstrap-runtime.yml`](../.github/workflows/bootstrap-runtime.yml) and can
also be dispatched manually. Certificate rotation thresholds are implemented in
[`bootstrap_runtime.py`](../.github/scripts/bootstrap_runtime.py).
Unrelated public keys and unexpired previous certificates are retained; only
this workflow's expired keys are eligible for removal after seven days.
The host configuration relies on Caddy's automatic HTTPS for
`talkwithtiles.com` and `www.talkwithtiles.com`, with certificates stored and
renewed in its persistent `/data`. Production changes and legacy cleanup
require separate authorization.

Missing established credentials, unexpected ownership, disabled secrets,
expiring database passwords or changed
certificate bindings stop publication and require explicit recovery. A failed
bootstrap can leave an unused public certificate; retry without deleting
existing keys. Verify real organisational and personal-account sign-in and
sign-out on the new domain before removing the legacy client-secret credential
or retiring Azure compute and storage.
