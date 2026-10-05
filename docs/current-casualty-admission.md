# Current-authority casualty admission

## Status

**Steering 016 evidence is historical for the review correction below.** Its
focused combat and complete migration CI passed on the earlier inputs, including
exports and both package smokes. Those passes do not validate the review's changed
inputs. The failed pause827/revision922 below remains the historical first divergence.
Gameplay, wave/death/equipment bounds and original deadlines are unchanged. See
[retained migration evidence](authored-migration.md).

Review found a separate missing acceptance constraint: body41 death818→866 at
current tick836 still has more than twelve ticks remaining, but its rendered death
pose is already beyond the required strict `PoseSeconds < .35` boundary. An accepted
pause at that tick cannot become fresh while frozen.

The shared eligible-body set now uses `CombatPlayback.DeathPose`, the body's sampled
frozen/current tick and its role/faction's death-clip endpoint from provenance-validated
GLB bytes. The same strict freshness predicate governs current eligibility, accepted
receipt eligibility, body selection, the rendered wait and frame validation. Focus,
deployment, declared lifetime and the focused living damaged target remain required.
The existing own-pause resume/re-arm path handles request-to-receipt aging without
new attempts, state or deadlines. Cheap executable controls cover tick836, adjacent
source-timing ticks831/832 for all eight rigs, the exact `.35` boundary, multiple-body
selection, witness/request/receipt aging and rendered freshness/visibility failures.

Review-round verification: **38 focused casualty/caller/frame tests passed**, zero
failed or skipped; locked solution restore, formatting restricted to the six changed
C# files and graphical-project compilation passed with zero warnings/errors. Evidence:
`logs/review-casualty-freshness/casualty.trx` and `verification-retry.log`. The first
attempt stopped at mise's worktree trust prerequisite before restore; the retry used
process-local trust with automatic installation disabled, not persistent tool settings.
No Godot process, full test/lint suite or full CI ran in this review round. New
current-frame combat evidence and full-run acceptance belong to the outer pipeline;
the retained historical passes below are not substituted for that evidence.

## Historical first divergence retained

Final full after-CI: `logs/20261005-111934-a7cc3f3b/`, failed after469.03s.
Rules and all six network/source economy/launcher/reconnect/settings checks passed;
source combat failed at111.19s, before exports. Its earlier independently selected
combat passed156.80s (`logs/20261005-111135-31569449/`): that is partial evidence,
not permission to ignore the failed complete gate.

At that failure, `CombatCheckpoint.PauseFreshCasualty` awaited
`CasualtyInspectionReady` against retained headless snapshots, then requested an
ordinary pause without revalidating eligibility against the actual pause receipt.
It later required a currently visible, living damaged enemy in the focused city
while the authority was frozen. That run's retained pause/UI state is
**tick827/revision922, Combat, paused**:

- City1 fresh dying body41: death818→866 (39 ticks remain).
- Living city1 enemies44/45: **3000/3000**, deployed/visible, undamaged.
- Damaged enemies46 (**600/4000**) and47 (**1400/4000**) belong to **city2**;
  their current rendered observations are hidden when city1 is focused.
- Latest received/applied/presented tick is the same827; this failure is not the
  old many-second native backlog or a claim that stale PNG bytes are current.
- The predicate `Visible && !Dead && Faction==Skeletons && Health<MaximumHealth`
  cannot succeed in this frozen city1 state. The UI wait reports
  `currently damaged opponent for live inspection`, not a weaker image acceptance.

Evidence: `combat-casualty.png`, `combat-casualty-observation.json`, and owned
`ui-client-*.log.states.json` (`RecentUi` and last state). Headless request/ack and
snapshot logs retain actual authority identity. The historical eligible witness's
exact arrival timestamp is not recorded here; do not invent its tick or claim a
packet-level race from unrelated clocks.

`HarnessTests.HistoricalCasualtyEligibilityDoesNotEstablishActualPauseReceiptEligibility`
reproduces the interface distinction cheaply. It uses retained actual pause facts
and explicitly controlled earlier eligibility, not reconstructed timing. Existing
`CasualtyCaptureCannotFreezeBeforeItsDamagedOpponentExists` already rejects foreign
city damage, expired/no corpses and full/dead targets. This recurring problem is
therefore caller admission/receipt coordination, not permission to relax that predicate.

## Smallest correction (implemented under steering 016)

Within the existing single ordered headless driver and unchanged deadline:

1. Arm/re-arm from a revision floor and check the latest current nonterminal combat
   state, rather than authorizing a pause merely from a retained historical match.
2. Request the ordinary supported pause only for a currently eligible focused-city
   casualty **and** living damaged enemy.
3. Validate that same requirement on the actual pause receipt. If an eligible target
   disappeared before the receipt, resume normally and re-arm; do not freeze and
   wait for new damage, change cities to substitute a foreign target, resample a
   historical picture or weaken death/equipment/inspection assertions.
4. Cheap controls must cover eligibility disappearing between witness/request/ack,
   rejected/terminal/stale quotes, bounded cancellation/owned-wait cleanup and an
   eligible current receipt preserving the inspection opportunity across PNG work.

`CasualtyAdmission.Pause` is exercised through the same executable caller interface
by the existing ordered driver and cheap controls. Retained revisions only wake it;
the latest state must be newer than its floor, unpaused, current Combat in the same
match/configuration/turn/wave and satisfy the unchanged predicate. Ordinary commands
retain the process's normal sequence reservation and immutable retry semantics.
New accepted readable acknowledgements must advance sequence and agree with the
quote's identity/tick/revision. The actual pause receipt and latest frozen state both
must remain eligible. A lost opportunity resumes only this driver's accepted,
still-current pause; it never undoes the initial graphical pause or a replacement
session. The fixture schedules no other pausing owner during this coordination;
its native-action adapter rejects an already-paused latest quote before sending.
A fresh resume revision becomes the next floor. **At most three pause attempts**
share one linked original `options.Timeout` deadline, with no renewal; the last
ineligible owned pause is resumed before cap failure. Cancellation awaits this single
waiter before peer/display teardown. Rejected, unreadable, duplicate/stale or terminal
receipts authorize neither capture nor a speculative resume.

`CasualtyAdmission.Frame` additionally binds current match identity, frozen tick,
revision and focused city to the same fresh visible body and living damaged enemy
ids/health from the accepted pause. This is checked on the fresh casualty observation
and the actual PNG checkpoint frame. Persistence cannot move the capture moment.
The ordinary click inspector and declared death-cleanup checks remain unchanged.

Cheap causal red/green controls: `logs/20261005-120201-a61e17a8/`, **740 tests**,
including 27 new caller/frame cases. The historical eligible control is accepted by
the old predicate while its current827 state is rejected; the real caller avoids
that pause, or resumes/re-arms when eligibility disappears at the receipt. Controls
also cover foreign/full/dead/hidden targets, expired/missing bodies, match/turn/
terminal/sequence/revision faults, cancellation, ownership loss and finite cap.
The source build/type checks and locked formatting passed.

One authorized owned combat: `logs/20261005-120254-70454a9b/`, **151.61s**.
Current request795/revision889 → accepted pause**797/revision892**, city1; damaged
enemy41 **1300/4000** and fresh body8 are present in the actual1280×720 casualty
frame. Body8 death-end840 is removed in the actual observed851 frame; presentation
elapsed0.40s after resume, below the original2s cleanup bound. Walking, all role
attacks/equipment, hit/recovery, focus/pause/resize, research and fresh-session cleanup
pass. `combat-casualty-admission.json`, `combat-casualty-pause.json`, the exact
PNG/observation and `combat-death-cleanup.json` retain current/request/receipt/frame
facts. This run needed one pause attempt; lost-at-receipt resume/re-arm is proven by
the cheap controls, not claimed as an observed live race.

Full after-CI `logs/20261005-122036-c53487dd/` passed **553.42s**, all gates.
Its preceding536.16s attempt passed casualty/source checks but exposed a separate
packed `.glb.import` inventory projection defect; ten strict source/import/remap
controls and selected package acceptance corrected it without admitting any missing,
legacy or unregistered model. Final cheap coverage is750. All original rules, source
hashes, notices and owned teardown remain; failed evidence is preserved.
