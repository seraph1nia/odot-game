# Proposal

## Why

The repository has mise and OpenSpec configured but no game project or verification workflow. A small C# multiplayer game will establish a reusable Godot foundation and make both interactive local testing and automated client/server verification straightforward before a larger game direction is chosen.

## What Changes

- Scaffold one Godot 4 .NET project with graphical client and headless dedicated-server modes, a small engine-independent C# rules library, and a C# test project.
- Add a 2D coin-collection demo with two colored players, server-owned movement and scoring, synchronized state, and connection feedback.
- Extend the existing mise setup to manage the .NET SDK and the Godot .NET distribution where supported, preserve existing OpenSpec tooling, and expose documented development, verification, and export tasks.
- Add repository conventions for formatting, line endings, generated files, Godot resource identifiers, and reproducible dependency resolution.
- Provide one command to build and run a local server and two clients, plus individual server/client commands with configurable endpoints and reliable process cleanup.
- Add unit tests and a bounded headless integration test that exercises real networked processes.
- Add GitHub Actions verification followed by Linux client/server exports only when all checks pass. Builds stay in the CI workspace; artifact uploads, releases, publishing, and deployment are deferred.

## Capabilities

### New Capabilities

- `development-tooling`: Repository setup, managed tools, dependency resolution, task entry points, and local client/server orchestration.
- `multiplayer-demo`: Desktop clients, a headless authoritative Godot server, and synchronized coin-collection gameplay.
- `automated-verification`: Unit and network integration tests, failure diagnostics, and a CI workflow that gates export builds on passing checks.

### Modified Capabilities

None. The project has no existing capability specs.

## Impact

Adds a Godot project and .NET solution, tests, a small process runner, repository configuration, README documentation, and a GitHub Actions workflow. Extends `mise.toml` without replacing its existing Node/OpenSpec entries. Introduces the .NET SDK, Godot .NET editor and matching export templates, and test packages resolved through NuGet. Desktop clients target Linux, Windows, and macOS; automated verification and export builds initially run on Linux x86_64.

The first milestone excludes browser/mobile exports, matchmaking, accounts, persistence, production hosting, release automation, and prediction/rollback networking. Existing OpenSpec configuration and generated skills remain unchanged.
