# Fresh primary recovery during intake

Date: 2026-10-03. Outcome: basic live restart/repeated-selection check passed;
full acceptance remains incomplete. The setup operator relayed user intent to the
single Pi primary; these were real native workers, not scripted worker outcomes.

The operator submitted Pi's `/new` to the idle owned primary while its existing
intake worker was active. Old session:
`2026-10-03T08-28-52-015Z_01a100e1-5dae-73a7-819f-6766da4684f7.jsonl`.
New session:
`2026-10-03T09-44-47-153Z_01a10126-df30-73a7-819f-676b7d919c16.jsonl`.
Both are retained under the selected FirstMate home's `state/pi-primary/`.
The primary remained in `odot-poc:w2:p1`. Herdr's agent nickname cleared on the
new session; the operator rebound `odot-primary` to that exact existing pane.
No worker, server, lease or worktree was destroyed to simulate recovery.

The fresh primary read project guidance, native backlog/meta/status/inbox/report
and Git state. It recovered `poc-scenario1-fuzzy-intake`, endpoint
`odot-poc:w4:p2`, native worktree
`/home/bart/.treehouse/odot-game-ab1b67/1/odot-game`, branch
`fm/poc-scenario1-fuzzy-intake`, and the committed documentation-guide shaping
choice. Recovery head was `1f47c9a4a17a795ddeec968a8e3653813df41099` with the
worker's report still staged. It did not dispatch a duplicate worker or infer
trial implementation approval. The Pi watcher tool reported an unchanged
extension-owned arm child. The slower worker subsequently completed at
`fe42f72766ec839ecead121f87d63c0123532e1d`, terminal timestamp1791021053;
elapsed1208s. Native completion inventory passed; the unlanded branch/lease
remains retained for FirstMate, rather than claiming cleanup or main landing.

From the registered clone, the primary ran the already-built DevRunner DLL's
`planning-next --json` twice, without restore/build/tests. It confirmed the
clone stayed clean at `eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7` before,
between and after the calls. Both returned `valid: true`, `change: null`, no
diagnostics, and identical exclusions: four legacy-completed changes, blocked
bootstrap and proposed research. Neither queue order nor selection changed.

The native report's final supplement and acknowledged `004.msg` preserve these
observations at the selected home's `data/poc-scenario1-fuzzy-intake/report.md`
and `state/poc-scenario1-fuzzy-intake.inbox/handled/004.msg`. The committed intake
report is [scenario-1-fuzzy-intake.md](scenario-1-fuzzy-intake.md).
One actual human shaping answer and four supervisor messages were recorded.
The operator also performed the new-session and nickname operations; fresh
conversation creation alone is not a process/server crash-recovery test.

No live trial implementation approval or trial review/repair count existed yet.
Recovery with those receipts remains to be demonstrated before closing full
restart task5.7. Scenarios2–5 and final acceptance remain open; no Atomic
evaluation or whole-change PASS is claimed. Game/source/tests/runner/specs
remained untouched by this live intake/restart exercise.
