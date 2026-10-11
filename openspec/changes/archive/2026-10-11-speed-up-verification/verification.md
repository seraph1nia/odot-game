# Archival disposition (2026-10-11)

This completed change is retained as historical proposal, design, checked tasks
and capability deltas. Its automatic graphical policy is superseded by the
[current testing strategy](../../../../.agents/skills/testing-strategy/SKILL.md)
and canonical capability specs. Original coverage, timings and failed/passing
attempts remain in
[verification history](../../../../docs/verification-history.md#verification-speed-implementation-coverage-ownership);
this archival operation makes no new execution or performance claim.

The installed OpenSpec 1.13.2 consumer performed:

```text
openspec archive speed-up-verification --skip-specs --json
```

`--skip-specs` prevents applying obsolete deltas to canonical requirements; it
does not disable archive delta validation. No `--no-validate`, `--yes`, metadata
waiver or manual validation bypass was used. The consumer reported
`specsUpdated: false` and moved the change to this existing date-prefixed archive
convention. All six original files, including `.openspec.yaml`, are retained;
only relative links were adjusted for the extra archive directory depth.
Historical requirements, scenarios, tasks and evidence claims were not rewritten.

After archival and link repair, whole-repository
`openspec validate --all --strict` passes **26 active items, zero failures**.
The completed historical change is no longer an active delta to apply. Canonical
spec bytes, gameplay code and meaningful test coverage are unchanged by archival.
Consumer output is retained in the lint worktree's `logs/lint-archive.json` and
`logs/lint-openspec.log`. No tests or graphical processes ran in this lint phase.
