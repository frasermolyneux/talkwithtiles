# Talk With Tiles

[![Build and Test](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/build-and-test.yml)
[![Code Quality](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/codequality.yml)
[![PR Verify](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/pr-verify.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/pr-verify.yml)
[![Deploy Dev](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/deploy-dev.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/deploy-dev.yml)
[![Deploy Prd](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/deploy-prd.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/deploy-prd.yml)

## Documentation

* [Architecture Overview](docs/architecture-overview.md)
* [Development Workflows](docs/development-workflows.md)

## Overview

Talk With Tiles is an ASP.NET Core 10 MVC word-game application. Players create games, manage contacts and accept invitations using personal Microsoft or organisational Entra accounts. The new constrained-container runtime uses dedicated PostgreSQL 18, certificate-backed authentication and a durable local Data Protection keyring.

The migration starts with a **fresh database**; Azure Table Storage data is not transferred. The initial `workload.json` deliberately contains non-deployable digest placeholders. Azure application deployment is blocked; legacy Terraform, identities and destroy workflows remain for separately coordinated retirement. No hosted Development environment is provisioned.

See [runtime, local startup and recovery](docs/container-runtime.md) and [development workflows](docs/development-workflows.md).

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
