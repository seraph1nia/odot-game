# versioned-distribution Specification

## Purpose
Deliver attributable Windows and Linux client builds to friends through versioned GitHub Releases after repeatable verification.

## Requirements

### Requirement: Tagged client version identity
Distribution SHALL build the tagged source revision for a validated `v`-prefixed SemVer version. The full version and source commit SHALL agree across packaged build metadata, displayed game version, installer information, and release assets. Multiplayer protocol identity SHALL remain independent of distribution version. Invalid or unsupported versions SHALL fail before export or publication. Untagged development launches SHALL remain usable and identify themselves as development builds.

#### Scenario: Build a preview tag
- **WHEN** distribution builds `v0.1.0-beta.1`
- **THEN** both client packages identify version `0.1.0-beta.1` and the same tagged source commit
- **AND** the GitHub release is marked as a prerelease

#### Scenario: Reject an invalid version
- **WHEN** a requested release tag cannot be represented by the supported version or platform metadata rules
- **THEN** distribution fails with the invalid input identified and publishes nothing

### Requirement: Complete desktop deliverables
Each release SHALL provide a Windows x86_64 installer, Linux x86_64 archive and version-specific install script, SHA-256 checksums, and public non-secret build metadata. Clients SHALL contain their executable, packed resources, required .NET runtime and assemblies, matching pinned Steam libraries, launch resources, and redistribution notices. Playing SHALL require neither Godot nor development tools. Linux archives SHALL preserve required executable permissions. Client installation SHALL NOT require a dedicated server package.

#### Scenario: Download a client on a machine without development tools
- **WHEN** a friend installs the package on a supported desktop with the game's documented OS prerequisites
- **THEN** the game launches without installing Godot, the .NET SDK, mise, or a separate server

#### Scenario: Incomplete platform package
- **WHEN** a required runtime, packed resource, native dependency, or notice is missing
- **THEN** package validation fails and the release is not published

### Requirement: Steam distribution identity and qualification
Installed clients SHALL retain Steam multiplayer and offline solo behavior. Preview test releases SHALL support the existing development AppID 480, clearly disclose their test status and outstanding Steam acceptance, and require players to start the game themselves before joining when genuine Steam cold launch is unavailable. Production releases SHALL require the game's own configured non-480 AppID and exclude development-only AppID settings/files. Distribution SHALL NOT claim that a local package check proves real Steam invitations, authenticated remote gameplay, relays, or cold launch. Missing Steam or its overlay SHALL leave solo and settings usable with existing multiplayer availability feedback.

#### Scenario: Share a friends-only preview
- **WHEN** a prerelease is packaged for testing with AppID 480
- **THEN** its clients retain the Steam multiplayer route and its release notes identify development identity and remaining acceptance limitations

#### Scenario: Missing production identity
- **WHEN** a production release is requested without the game's own valid AppID
- **THEN** distribution fails instead of silently shipping AppID 480

### Requirement: Gated GitHub publication
A separate release workflow SHALL run when the owner manually publishes an empty release for an existing version tag, verify the exact tagged source, and attach assets after both platform builds and identity/checksum checks pass. It SHALL NOT rerun gameplay, UI, network, or installation test suites. Public downloads SHALL reside in `seraph1nia/odot-game`; the repository SHALL be public before publication. Build jobs SHALL use locked tools/dependencies and validated matching export templates. Shared build/import/export mutations SHALL remain sequential within each workspace. Existing published assets SHALL NOT be overwritten or silently rebuilt in place.

#### Scenario: A source or package gate fails
- **WHEN** release preflight, a platform export, installer build, identity check, or checksum check fails
- **THEN** no distributable assets are attached to the manually published release

#### Scenario: Retry a completed release
- **WHEN** publication is requested for a version that already has a completed release
- **THEN** the workflow reports the existing version without replacing its assets

#### Scenario: Repository remains private
- **WHEN** publication preflight finds that the repository has not been made public
- **THEN** publication fails with the prerequisite identified and does not change repository visibility
