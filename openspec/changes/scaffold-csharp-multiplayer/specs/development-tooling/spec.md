# Spec Delta

## Purpose

Give contributors a documented, repeatable desktop development setup and simple commands for building, running, and testing separate game clients and servers.

## ADDED Requirements

### Requirement: Managed development environment
The repository SHALL declare the development tools required for C# Godot development through `mise.toml` where supported, retain its existing OpenSpec tooling, and record the resolved versions used by local verification and CI. Project dependencies SHALL be versioned within the repository. Setup documentation SHALL distinguish developer tools, game dependencies, matching export templates, and any operating-system prerequisites.

#### Scenario: Fresh developer setup
- **WHEN** a contributor follows the documented setup on a supported desktop platform
- **THEN** the C#-capable Godot editor and .NET SDK are available to repository tasks without machine-specific paths committed to source control
- **AND** the existing OpenSpec commands remain available

#### Scenario: Dependency mismatch
- **WHEN** the required C# engine distribution, compatible SDK, or matching export templates are missing or incompatible
- **THEN** the relevant task fails with actionable diagnostics before reporting a successful run or export

### Requirement: Repository source conventions
The repository SHALL include documented source organization, formatting and line-ending conventions, and ignore rules for generated engine caches, .NET build outputs, local settings, logs, and exported binaries. Source scenes, assets, C# project files, dependency declarations, and engine resource identifiers required to reopen the project SHALL remain versioned.

#### Scenario: Reopen a clean checkout
- **WHEN** a contributor prepares a clean checkout using the documented commands
- **THEN** the game opens and builds from tracked source and declared dependencies
- **AND** regenerated caches and build outputs do not become candidate source changes

### Requirement: Stable task interface
The repository SHALL expose `mise run dev`, `mise run server`, `mise run client`, `mise run test`, `mise run test-network`, and `mise run ci`. Server binding and client connection endpoints SHALL be configurable through documented arguments. Local defaults SHALL connect to loopback.

#### Scenario: Launch roles independently
- **WHEN** a contributor starts a server and a client with matching explicit endpoint arguments
- **THEN** the server runs without a graphical window and the client connects to that server

#### Scenario: Run commands outside an IDE
- **WHEN** a contributor invokes the documented task commands from the repository root
- **THEN** preparation and execution require no IDE-specific launch configuration

### Requirement: Local multiplayer orchestration
`mise run dev` SHALL prepare the project, launch one headless server, wait for server readiness, and launch two graphical clients connected to it. The command SHALL show attributable process output and terminate every child it starts when interrupted or when startup fails.

#### Scenario: Interactive multiplayer session
- **WHEN** a contributor runs `mise run dev` with the managed tools available
- **THEN** two client windows display distinct players in the same running game
- **AND** stopping the command shuts down its server and clients

#### Scenario: Server startup failure
- **WHEN** the requested server port cannot be bound or the server exits before becoming ready
- **THEN** orchestration exits unsuccessfully with a useful diagnostic
- **AND** it cleans up its children and does not leave waiting client processes running
