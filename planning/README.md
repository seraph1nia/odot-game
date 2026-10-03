# Project planning

Git is the durable record. Conversation helps clarify decisions but does not replace these files.

| Artifact | Question answered |
| --- | --- |
| `ideas/*.md` | Should we consider doing this? |
| `openspec/changes/<id>/` | What exactly are we proposing? |
| `roadmap.yaml` | In what order should approved changes run; what hard dependencies exist? |
| `docs/adr/` | What project-wide decisions should future work respect? |
| `openspec/specs/` | What does the project currently claim to guarantee? |
| Source and tests | What does it actually do? |
| FirstMate private home | What are workers doing right now? |

FirstMate's backlog is operational notes with pointers to project artifacts. It cannot become a second product roadmap. Intake and proposals may be prepared automatically. First implementation needs explicit user approval for one change and its current spec inputs. Archive may be prepared after current independent PASS, complete tasks, successful applicable validation and no outstanding decision. Landing always needs separate explicit approval.

## Ideas

Copy `templates/idea.md` into `ideas/IDEA-NNN-short-title.md`; replace the example ID and date. Stable IDs survive moves to `archive/ideas/`. Allowed statuses are `inbox`, `exploring`, `shaped`, `promoted`, `parked`, `rejected`. Consideration does not automatically create a change. `promoted` requires at least one valid `related_changes` entry and reciprocal `source_ideas` roadmap links. Duplicate IDs across active/archive locations fail validation. Archive idea files without discarding decisions, links or supersession history. `README.md` is the only non-idea Markdown file allowed in these two directories.

## Roadmap version 1

Required root fields: `version: 1`, `queue` array. Each item requires `change`, `state`, `priority`, `source_ideas`, `depends_on`, `blocked_by`, `conflicts_with`, `notes`. Only optional item fields are `approval` and `verification`, pointing to Markdown receipts under `evidence/<change>/`. Strict YAML rejects unknown fields, duplicate keys/list values, anchors, aliases, tags, malformed scalars and unsafe/symlink paths. Idea frontmatter uses the exact fields in the template; dates are YYYY-MM-DD. YAML is parsed by the intentionally added, locked YamlDotNet dependency in DevRunner.

Queue array order controls selection. Priority (`low`, `normal`, `high`) is descriptive. A hard dependency must precede its dependent; priority never overrides it. Active dependencies/conflicts need queue entries. Historical dependencies can resolve from exact `YYYY-MM-DD-change-id` archives outside the queue. Multiple archive dates or active/archive duplicates for one ID are ambiguous and rejected. Blockers are unresolved human-readable strings: remove them only with a recorded resolution. A conflict remains unresolved until the referenced change is completed/archived/legacy-completed; record any alternative resolution before removing the link.

| State | Meaning |
| --- | --- |
| `proposed` | Planning exists or is incomplete; no approval implied |
| `ready` | Explicit implementation approval matches spec inputs; all planning is complete |
| `in_progress` | Exactly one approved change is being implemented |
| `blocked` | Work awaits a prerequisite or decision; describe it in blockers/notes |
| `verified` | Current fresh Reviewer PASS; not necessarily landed |
| `completed` | Verified work landed in local `main`; dependencies can clear |
| `archived` | Completed with OpenSpec archive resolved |
| `legacy_completed` | Imported pre-workflow completed tasks; no fabricated PASS |

Imported baseline entries reflect inspected tasks, not CLI's directory/task-presence status. At bootstrap base `4a70e16`, optimization is already 43/43; its stale in-progress planning snapshot is historical. The four completed active entries are imported as `legacy_completed`; research remains unapproved `proposed`. Bootstrap apply approval is explicit. Its implementation checkpoint must become `blocked` awaiting live acceptance after the worker stops, before sequential trial implementations. Trials reference the checkpoint in notes, never pretend this unfinished change is a completed hard dependency. Unapproved future proposal order is provisional and does not imply product priority.

## Deterministic commands

```sh
mise run planning-validate
mise run planning-validate -- --json
mise run planning-next
mise run planning-next -- --json
mise run planning-inputs -- CHANGE-ID
```

These build only DevRunner with locked restore, use no engine/runtime credentials and start no Godot. `planning-next` validates first, then returns the earliest `ready` item with all dependencies finished, no unresolved blockers/conflicts and no `in_progress` implementation. Identical state yields identical selection/explanations. `change: null` is successful when no item qualifies. Invalid planning returns exit 1 and artifact/change diagnostics. JSON contains `valid`, `change`, `diagnostics`, `exclusions`; selection never mutates files. The primary must additionally reconcile FirstMate live workers before dispatch. The source CI gate invokes the same validator before engine preparation.

## Evidence and revision identity

Copy receipt templates from `templates/` into `evidence/<change>/`. Reports must include actual user authorization, workers/session/worktree IDs, exact Git base/head, standalone handoff/findings and command/result summaries. Raw non-secret logs go under ignored `logs/<run-id>/`; no credentials or copied conversations. Fixture outcomes never count as live evidence.

`planning-inputs CHANGE-ID` outputs current `spec_digest`, `code_digest`, `head` and `canonical_inputs`. Approval binds to the change artifact digest. Review binds to those inputs plus exact available Git base/head and distinct Shipper/Reviewer task IDs. Review attempt is 1 or 2; one repair only. Task checkbox status is normalized in `spec_digest`, so completing tasks does not change approved requirements. Other task text and proposal/design/deltas/metadata remain covered. Code digest covers Git-tracked and nonignored new files except `planning/` and `openspec/`; engine/cache/log ignored output does not count. Production inputs must match reviewed committed head; uncommitted production inputs invalidate PASS. All canonical spec file hashes are required in review receipts.

`verified`/`completed`/`archived` require PASS, complete tasks, successful validation, resolved decisions and current digests. Completed/archived historical receipts validate code and canonical hashes at their immutable reviewed head, allowing separately authorized later work without invalidating past completion. Current verified/archival admission still checks current inputs. Completion also requires `landed_revision`, reviewed head ancestry and landed revision reachable from current checkout and local `main`. Merely preparing archive bookkeeping on a branch is not landed completion. Historical archives outside the queue are treated as pre-workflow completion; newly introduced workflow entries cannot use that exemption to avoid their receipts. `legacy_completed` is migration only, never a path around review for new work.

Archive relocation and receipts do not alter reviewed production/requirements. Canonical sync changes file hashes: retain the original report, explicitly reconcile equivalent synchronized guarantees in the lifecycle report and refresh binding through independent review if the hash check changes; do not silently edit a receipt to green. Material source/spec changes always invalidate prior PASS/approval. Digest/identity checks are deterministic consistency checks, not cryptographic proof of user or agent identity; the primary owns actual approval and reviewer independence.

Run `openspec validate CHANGE-ID --strict` as well: this planning validator does not replace OpenSpec content validation. Lifecycle still uses the existing sync/archive skills and archive's sync behavior, without duplicate syncing. See [operator runbook](../docs/agent-workflow.md) and [scenario procedures](../docs/agent-workflow-scenarios.md).
