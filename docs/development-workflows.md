# Development Workflows

## Branch Strategy

The repository follows a trunk-based development model with short-lived feature branches:

- **`main`** - Production branch. Merges to main trigger the full deployment pipeline.
- **`feature/*`** - New feature development branches.
- **`bugfix/*`** - Bug fix branches.
- **`hotfix/*`** - Urgent fix branches.

## CI/CD Workflows

### Build and Test (`build-and-test.yml`)

Triggers on pushes to `feature/*`, `bugfix/*`, and `hotfix/*` branches. Builds the .NET solution and runs all unit tests to provide fast feedback during development.

### PR Verify (`pr-verify.yml`)

Triggers on pull request events (opened, synchronize, reopened, ready_for_review, labeled, unlabeled) excluding drafts. Runs the build-and-test step followed by a Terraform plan against the dev environment to verify infrastructure changes. Skips the Terraform plan for Dependabot PRs with the `deploy-dev` label.

### Deploy Prd (`deploy-prd.yml`)

Triggers on pushes to `main`, manual dispatch, or a weekly schedule (Thursday at 3 AM UTC). Runs the full deployment pipeline:

1. Build and test the application
2. Terraform plan and apply to the **dev** environment
3. Terraform plan and apply to the **prd** environment

Each environment deployment uses concurrency groups to prevent parallel deployments to the same environment.

### Deploy Dev (`deploy-dev.yml`)

Triggers on manual dispatch. Deploys to the dev environment independently, useful for testing infrastructure changes before merging to main.

### Code Quality (`codequality.yml`)

Runs on a weekly schedule (Monday at 3 AM UTC) and on manual dispatch. Performs code quality analysis.

## Typical Development Flow

1. Create a `feature/*`, `bugfix/*`, or `hotfix/*` branch from `main`
2. Push commits — `build-and-test.yml` runs automatically
3. Open a pull request — `pr-verify.yml` runs build, test, and Terraform plan
4. Merge to `main` — `deploy-prd.yml` deploys through dev then prd
