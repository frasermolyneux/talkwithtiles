# Talk With Tiles

[![Build and Test](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/build-and-test.yml)
[![Code Quality](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/codequality.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/codequality.yml)
[![PR Verify](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/pr-verify.yml/badge.svg)](https://github.com/frasermolyneux/talkwithtiles/actions/workflows/pr-verify.yml)

## Documentation

* [Architecture Overview](docs/architecture-overview.md)
* [Development Workflows](docs/development-workflows.md)
* [Container runtime, local setup and recovery](docs/container-runtime.md)
* [Production runtime credentials](docs/runtime-credentials.md)

## Overview

Talk With Tiles is an ASP.NET Core MVC word-game application. Players create games, manage contacts and accept invitations using personal Microsoft or organisational Entra accounts. Its constrained-container runtime uses dedicated PostgreSQL, certificate-backed authentication and a durable Data Protection keyring.

The migration starts with a **fresh database**; Azure Table Storage data is not transferred. Azure App Service deployment is blocked. Image promotion and host activation are separate operations; see the runtime and workflow guides above.

## Contributing

Please read the [contributing](CONTRIBUTING.md) guidance; this is a learning and development project.

## Security

Please read the [security](SECURITY.md) guidance; I am always open to security feedback through email or opening an issue.
