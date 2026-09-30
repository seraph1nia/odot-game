# Spec Delta

## ADDED Requirements

### Requirement: Strict C# compilation and style gates
Every repository C# project SHALL enable nullable reference analysis and implicit usings, and compiler or analyzer warnings SHALL fail compilation. Ordinary builds SHALL run the recommended code-quality analyzers supplied by the declared locked SDK and enforce configured C# style warnings, including file-scoped namespaces and readonly fields where possible. Reported findings SHALL be fixed rather than excluded, suppressed or addressed by weakening enforcement. Tool and dependency locks SHALL remain unchanged unless intentionally updated.

`mise run check` SHALL verify solution formatting without modifying source. CI SHALL require locked restore, formatting verification and strict solution compilation before source tests that depend on compiled outputs. Both cheap xUnit suites, all existing headless Godot network scenarios and source private-display UI slices SHALL gate exports; headless and graphical package smoke SHALL gate overall success after exports. Local checks and CI SHALL apply the same compiler, analyzer and style policy while preserving shared preparation and selectable expensive slices.

#### Scenario: Nullable code fails compilation
- **WHEN** C# source returns null from a non-nullable reference return type
- **THEN** an ordinary build fails with an attributable nullable diagnostic

#### Scenario: A code-quality finding fails compilation
- **WHEN** C# source violates an enabled recommended code-quality analyzer rule
- **THEN** an ordinary build fails with the analyzer diagnostic until the finding is fixed

#### Scenario: A configured style finding fails compilation
- **WHEN** a C# field can be readonly but is declared mutable
- **THEN** an ordinary build fails with the configured style diagnostic until the declaration is fixed

#### Scenario: Formatting drift fails verification
- **WHEN** checked-in C# formatting differs from repository formatting rules
- **THEN** `mise run check` and CI's formatting gate fail without modifying source, and CI does not proceed to source verification or exports

#### Scenario: Strict checks cover engine and supporting projects
- **WHEN** local commands or CI compile the game, engine-independent core, runner or test projects
- **THEN** each applies the shared strict policy, and game verification and exports proceed only after their required gates pass
