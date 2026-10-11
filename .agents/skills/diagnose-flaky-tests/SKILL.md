---
name: diagnose-flaky-tests
description: Diagnose intermittent odot-game test/CI failures, timeouts, cancellation, readiness, resource contention or cleanup. Load before changing synchronization or a deadline.
---

# Evidence-led diagnosis

Start from the exact source SHA, locked tools, selected command, process budget,
seed and cache state. Read `logs/<run-id>/*-summary.json`, per-phase JSON, child
engine logs and bounded redacted state evidence. Use forge run/job logs to inspect
historical failures without rerunning forbidden graphical journeys. Keep historical
and current executions separate; unlike hosts/import caches are not a benchmark.

Find the first causal failure, not just its final symptom:
- **Initiator:** assertion, readiness failure, owned deadline or external cancellation.
- **Masking event:** sibling cancellation, cleanup exception, missing artifact upload.
- **Symptom:** worker exit 130, no later frame, generic suite cancellation.

Classify product regression vs incorrect assumption/fixture, race, shared-state
interference, resource contention, scheduling/readiness, real timeout, rendering
assumption, setup failure or inadequate cleanup. Missing prerequisites are
unexecuted/nonzero; do not call them passes or turn them into silent skips.

Choose the cheapest controlled discriminant: direct core transition; focused
headless component; one transport scenario; only if authorized, one graphical
checkpoint. State what would disconfirm the hypothesis. Parent cancellation can
shorten a feature's deadline: compare actual phase start, parent expiry, feature
expiry and worker admission before blaming the engine or application. A passing
pose check followed by cancelled research does not prove broken combat.

Check one ordered driver per child, current vs retained event predicates, command
receipt identity, revision/tick barriers, native connection readiness and frame
capture ids. Replace sleeps with explicit events/states. Check owned preferences,
credential restart paths, port leases, ECS/static registry state, admission and
awaited cleanup. Prepare shared outputs once before binary concurrency. Compare
`--jobs 1` and default only as a diagnostic; serial execution is not a permanent
workaround for unsafe ownership.

Repeat a focused case after a concrete synchronization fix or while discriminating
between hypotheses. Record every result and costs; repetition does not transform
an unproven cause into certainty. Never blind-retry until green, swallow assertion
failures, relax invariants, raise all deadlines or diagnose host/RAM/runtime faults
from an old null-read/hash/cancellation alone. Retain useful bounded non-secret
evidence; optional upload absence must not cause a secondary CI failure.

Implement only the demonstrated boundary fix; use a small regression test if it
can catch the defect deterministically. If graphical reproduction is prohibited,
record the narrowed evidence and unverified hypothesis explicitly. Follow
[testing-commands](../testing-commands/SKILL.md) for permitted selectors and the
[audit's historical failure ledger](../../../docs/testing-audit.md#failure-history).
