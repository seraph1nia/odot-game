---
name: testing-hygiene
description: Add, change, consolidate, delete or review odot-game tests and fixtures. Load before expanding a suite or moving coverage between tiers.
---

# Coverage admission and removal

Before changing a test record, in its code comment or change receipt:
1. The meaningful production defect it catches and its observable outcome.
2. Why existing cheaper coverage misses it (or why this test is redundant).
3. The smallest tier from [testing-strategy](../testing-strategy/SKILL.md).
4. Deterministic inputs, state setup, relevant synchronization and owned resources.
5. Expected execution/setup/maintenance cost; distinguish measured from estimated.

Prefer strengthening an existing assertion over adding another journey. Keep
small boundary/overflow/malformed-input/serialization/ownership tests even when
short. Assert production outcomes, not the test fixture's constants or a copy of
production arithmetic. Avoid exact config-text policy tests, hardcoded catalogs
of test names and broad opaque snapshot hashes in routine A. Protocol golden
vectors and stable correctness work bounds can be meaningful exceptions.

Use standard xUnit traits and .NET filters. `Tier=Deep` is owned by the actual
expensive C# test class, not a second name-to-tier catalog. Keep ECS-sensitive
core tests serialized inside their process; parallelize isolated binaries, not
shared static registry lifetimes. Shared read-only asset parsing is fine; do not
cache mutable match/preferences/session fixtures globally.

For Godot use the checked-in minimal headless component scene and real production
nodes, signals, imported resources and animation playback. Never add a full-game
UI test if direct core/component verification can catch the defect. A graphical
smoke has one purpose, explicit permission, bounded owned setup and cleanup;
comprehensive journeys remain selected E, not ordinary PR coverage.

When deleting or consolidating, update the
[coverage ledger](../../../docs/testing-audit.md): behavior removed, actual
replacement/overlap, preserved negative paths, and remaining limitation. Do not
claim a unit test proves rendering or a component proves real network delivery.
Review cancellation, cleanup, failure propagation, endpoint ownership and
credential isolation before accepting a smaller suite. Run the affected targeted
check, then broader A once coherent; use B/C only for relevant boundaries.
