# Development workflows

- Pushes to the feature, bugfix and hotfix branch patterns run `build-and-test`
  with change detection; non-draft PRs run PR Verify and its test summary.
- Runtime and Playwright inputs trigger Runtime Quality on those branch pushes;
  non-draft PRs call it after the build.
  PR concurrency is isolated by exact head, never cancelling an older stateful run.
- `e2e-tests` calls Runtime Quality: real PostgreSQL persistence tests,
  single-worker deterministic Playwright multiplayer flows, builds of both
  constrained images and read-only/non-root persisted-restart checks.
- PR Runtime Quality never receives private App credentials or executes private
  contract code, including for same-repository Dependabot PRs.
- `Validate Workload Contract` runs only through default-branch `workflow_run`.
  It checks out reviewed default-branch code/policy and private compiler
  at the pin in the workflow, fetching only the tested SHA's
  `workload.json` as data (also for forks and Dependabot). Its isolated Python
  never imports PR-controlled workspace files. The Production-only read-only App
  token is not forwarded to a PR workflow. Its validation fixture grants no host
  authority; parent-owned host policy is final.
- Existing Code Quality, Sonar, dependency review and DevOps security scanning
  remain unchanged. Unit-only runners skip PostgreSQL tests when
  `TEST_POSTGRES_PASSWORD` is absent; Runtime Quality always supplies it.
- Runtime Quality also runs on `main` and explicit dispatch. Image input changes
  produce an exact-source publication marker. Digest-only/documentation changes
  cannot recursively publish images.
- Publish Image consumes only successful same-repository `main` push validation,
  additionally requires exact-source Code Quality and `workload-contract` from
  GitHub Actions. It verifies the latter's trusted workflow-run producer
  and SHA-bound job, not just its name or App slug. Missing, spoofed, stale or
  failed results fail closed before cloud login/publication. It builds both
  images, uses OIDC with repository-scoped ACR tokens, and locks source tags and
  digests against overwrite/deletion.
- Promote Image opens a digest-only PR. It rejects newer source/image changes,
  requires exact-head dispatched Runtime Quality, PR Verify, Code Quality and
  Copilot review without unresolved threads. It **never merges or deploys**.
  Promotion requires the repository GitHub App credentials and Production ACR
  variables configured in repository Actions settings.

Azure Deploy Dev/Deploy Prd fail explicitly on dispatch and have no push or
scheduled triggers: PostgreSQL builds must not overwrite the old App Service.
Development plans are disabled. PR Verify has an opt-in Production plan via
`run-prd-plan`; Runtime Infrastructure separately plans eligible Terraform PRs
and applies only on approved `main` dispatch. Review workflow permissions and
outputs before use. Destroy workflows are operator-only. Retirement and host
activation require separate authorization.

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

## Script tests

```powershell
python -m unittest discover -s .github\scripts -p 'test_*.py'
python -m unittest discover -s deployment -p 'test_*.py'
```
