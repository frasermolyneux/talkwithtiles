# Development workflows

- Feature pushes retain `build-and-test`; PR Verify retains its build/test summary.
- Docker, deployment, manifest and CI changes also execute runtime validation.
  PR concurrency is isolated by exact head, never cancelling an older stateful run.
- `e2e-tests` calls Runtime Quality: real PostgreSQL 18 persistence tests,
  single-worker deterministic Playwright multiplayer flows, builds of both
  constrained images and read-only/non-root persisted-restart checks.
- `validate-contract` compiles the manifest using the pinned private platform
  compiler and `.github/validation/policy.json`. This fixture grants no host
  authority and cannot activate placeholders; parent-owned host policy is final.
  The read-only contract checkout requires the Production GitHub App installation
  to include `platform-baremetal`; private contract source is never published.
- Existing Code Quality, Sonar, dependency review and DevOps security scanning
  remain unchanged. Unit-only runners skip PostgreSQL tests when
  `TEST_POSTGRES_PASSWORD` is absent; Runtime Quality always supplies it.
- Runtime Quality also runs on `main` and explicit dispatch. Image input changes
  produce an exact-source publication marker. Digest-only/documentation changes
  cannot recursively publish images.
- Publish Image consumes only successful same-repository `main` push validation,
  additionally waits for successful exact-source Code Quality, builds both
  images, uses OIDC with repository-scoped ACR tokens, and locks source tags and
  digests against overwrite/deletion.
- Promote Image opens a digest-only PR. It rejects newer source/image changes,
  requires exact-head dispatched Runtime Quality, PR Verify, Code Quality and
  Copilot review without unresolved threads. It **never merges or deploys**.
  Promotion requires the repository GitHub App credentials and Production ACR
  variables provisioned by the platform catalog.

Azure Deploy Dev/Deploy Prd now fail explicitly on dispatch and have no push or
scheduled triggers: PostgreSQL builds must not overwrite the old App Service.
Development plans are disabled; a retained Production plan is opt-in through
`run-prd-plan` for retirement work. Terraform resources, app registrations,
provider locks and both destroy workflows are retained unchanged. Retirement and
host activation are separately reviewed operations, not application CI effects.

## Local commands

See [container-runtime.md](container-runtime.md) for the disposable database setup.

```powershell
dotnet build src\MX.TalkWithTiles.slnx
dotnet test src\MX.TalkWithTiles.slnx
dotnet format src\MX.TalkWithTiles.slnx --verify-no-changes
npm ci
npx tsc --noEmit
npx playwright test --project=chromium
```

The Playwright test-runner MCP in `.vscode/mcp.json` uses the pinned local package.
Do not enable a second Playwright server for the same workspace.
