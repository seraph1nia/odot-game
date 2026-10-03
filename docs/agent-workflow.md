# Single-project agent workflow POC

The local bootstrap adds deterministic Git-backed planning and scoped skills. The proposed runtime is one Pi/FirstMate primary, Herdr visible sessions, Treehouse native worktree isolation and fresh Codex Scouts/Shippers/Reviewers. FirstMate handles execution and guarded local landing; OpenSpec/planning/ADRs/source/tests own product truth. The five real-session scenarios are still unexecuted. Bootstrap tests are not live acceptance.

## Runtime and supplied paths

Inspected FirstMate checkout: `/home/bart/projects/personal/firstmate`, revision `e31bc6e620ca532c2e0e0b72f3fd7c0869a12270`. Pi1.0.0, Herdr0.9.1, Codex0.160.0 with Codex integrationv8 and Treehouse3.1.1 with `get --lease` were supplied by the user during bootstrap. These are observed versions, not repository dependency upgrades. Recheck actual help/contracts after changing tools. The tracked launcher supports that checkout's `.pi/extensions`, `fm-session-start.sh`, `fm-project-mode.sh`, `fm-brief.sh` and `fm-spawn.sh`; no `firstmate` executable is required.

Universal upstream bootstrap diagnostics still name missing `no-mistakes`>=1.46.0, `gh-axi`>=0.1.29, `chrome-devtools-axi` and `quota-axi`>=0.1.51. The supported selected route uses manual operational backlog, single static Codex dispatch objects and local-only/yolo-off delivery; these missing tools are reported and routes depending on them must not run. Native completion nevertheless requires compatible `tasks-axi`>=0.2.6, even for a Scout and manual backlog. The user authorized installation of0.2.6, and FirstMate's compatibility probe and captain-hold completion passed. Its runtime records remain separate from the Git product roadmap. GitHub authentication remains required by the upstream primary. Do not run bootstrap as a read-only doctor: it can mutate its home and fleet. Lavish is not required for text reports. Pi integrationv9, checkout trust, the native Pi watcher and a complete visible isolated Codex Scout lifecycle have been observed; see the tracked [completion evidence](../planning/evidence/setup-agent-workflow-poc/native-completion.md). The five product scenarios and restart acceptance remain open.

Upstream references: [configuration](https://github.com/kunchenguid/firstmate/blob/e31bc6e620ca532c2e0e0b72f3fd7c0869a12270/docs/configuration.md), [project mode contract](https://github.com/kunchenguid/firstmate/blob/e31bc6e620ca532c2e0e0b72f3fd7c0869a12270/bin/fm-project-mode.sh), [spawn/base contract](https://github.com/kunchenguid/firstmate/blob/e31bc6e620ca532c2e0e0b72f3fd7c0869a12270/bin/fm-spawn.sh), [Herdr backend](https://github.com/kunchenguid/firstmate/blob/e31bc6e620ca532c2e0e0b72f3fd7c0869a12270/docs/herdr-backend.md). Actual supplied source/help, not current online main, owns compatibility.

## Owned filesystem setup

Use ignored `mise.local.toml` or environment for local paths. Nothing in the launcher installs tools, modifies global preferences, clones FirstMate or creates worker sessions. `--init` only writes selected empty Odot POC home configuration and refuses a nonempty home. Never repurpose unrelated FirstMate state.

```sh
export ODOT_FIRSTMATE_ROOT=/home/bart/projects/personal/firstmate
export ODOT_FIRSTMATE_HOME=/home/bart/.local/share/firstmate/odot
export ODOT_HERDR_SESSION=odot-poc
bash tools/AgentWorkflow/agent-primary --init
```

Wait for the bootstrap implementation to be committed and independently checkpoint-reviewed. Do not copy dirty files into a worker base. Register a NEW clean owned clone from that checkpoint, retaining provenance under `odot-source` and deliberately omitting `origin`. Native FirstMate fresh spawn fetches/resets worktrees to origin/default when origin exists; leaving the developer origin would erase unlanded bootstrap files. Its supported no-origin path preserves the local default base. Only operate on this unused destination; do not reset/repoint an existing developer clone.

```sh
# Run in the clean committed bootstrap worktree; exact ID goes in setup evidence.
export ODOT_BOOTSTRAP_REVISION=$(git rev-parse HEAD)
git clone --no-local --single-branch --branch work/agent-workflow-bootstrap \
  --origin odot-source "$PWD" "$ODOT_FIRSTMATE_HOME/projects/odot-game"
git -C "$ODOT_FIRSTMATE_HOME/projects/odot-game" branch -m main
test "$(git -C "$ODOT_FIRSTMATE_HOME/projects/odot-game" rev-parse HEAD)" = "$ODOT_BOOTSTRAP_REVISION"
test -z "$(git -C "$ODOT_FIRSTMATE_HOME/projects/odot-game" status --porcelain)"
FM_HOME="$ODOT_FIRSTMATE_HOME" "$ODOT_FIRSTMATE_ROOT/bin/fm-project-mode.sh" odot-game
# Must print: local-only off
bash tools/AgentWorkflow/agent-primary --check
```

Templates installed into that selected home are `config/backend=herdr`, `crew-harness=codex`, `backlog-backend=manual` and `crew-dispatch.json` with three natural-language single Codex profiles and a Codex default. The native registry row is `- odot-game [local-only] - ...`; no +yolo. No model pin, dispatch-array/quota resolver, typed RPC resolver or product tracker. Dispatch must still pass `--harness codex` explicitly. The launch check refuses config drift rather than overwriting it. Initial setup must exactly match the checkpoint; normal startup accepts clean local-main descendants of that recorded checkpoint so approved native landings survive restart. Retain initial and new landed identities in Git setup evidence.

## Start the single primary and observe

The user provides Pi trust/login and the Herdr integration. These commands are user setup, not worker supervision:

```sh
herdr integration install pi
herdr session attach odot-poc
```

In the owned Herdr shell pane, set the four environment values above, change to the registered clone and run:

```sh
cd "$ODOT_FIRSTMATE_HOME/projects/odot-game"
mise run agent-primary
```

The launcher requires actual inherited `HERDR_ENV=1`, session/pane/tab/workspace/socket identities and matching session ownership. It reuses FirstMate's native fm_backend_herdr_launcher_identity to prove the canonical socket and live pane/tab/workspace relationships in the explicit named session. Setting only HERDR_SESSION does not start a pane. It invokes Pi from the supplied FirstMate checkout, so native `.pi/extensions` load, appends the tracked Odot orchestrator prompt, loads registered Odot `.agents/skills`, and stores primary conversations in selected-home `state/pi-primary`. It does not auto-approve extension trust or log in. Accept only the intended checkout's extensions through Pi's UI. On startup, the primary uses native `fm-session-start.sh`, then the Pi `fm_watch_arm_pi` extension tool; it must not substitute direct shell watch arming. The observed primary is named `odot-primary` in pane `odot-poc:w2:p1`; IDs describe this run and must be rediscovered after recreation.

Attach another terminal with `herdr session attach odot-poc` to observe workers. Inside a managed pane use `herdr agent list` and `herdr workspace list`; inspect returned immutable IDs rather than assuming label shapes. Native FirstMate `FM_HOME=... <root>/bin/fm-peek.sh <task-id>` collects task output. The user never needs to steer worker panes manually; the primary records returned task/session/worktree identities.

## Native worker shapes

These are the primary's existing FirstMate operations, not commands the user must manage. Read actual upstream brief instructions and fill BOTH Captain's intent and Firstmate spec, leaving no placeholders. Intake/preflight/review use `--scout`; a planning-only logical Scout writing Git artifacts uses native local-only Ship delivery with an explicit planning-path allowlist. This keeps four logical roles.

```sh
FM_HOME="$ODOT_FIRSTMATE_HOME" "$ODOT_FIRSTMATE_ROOT/bin/fm-brief.sh" poc-intake-001 odot-game --scout
# Primary fills the generated brief with project-intake and standalone report scope.
FM_HOME="$ODOT_FIRSTMATE_HOME" "$ODOT_FIRSTMATE_ROOT/bin/fm-spawn.sh" \
  poc-intake-001 projects/odot-game --scout --harness codex --backend herdr

# Only after explicit approval for ONE resolved change:
FM_HOME="$ODOT_FIRSTMATE_HOME" "$ODOT_FIRSTMATE_ROOT/bin/fm-brief.sh" poc-ship-001 odot-game --mode local-only
# Primary fills authoritative change/approval/base/validation/handoff contract.
FM_HOME="$ODOT_FIRSTMATE_HOME" "$ODOT_FIRSTMATE_ROOT/bin/fm-spawn.sh" \
  poc-ship-001 projects/odot-game --mode local-only --yolo off --harness codex --backend herdr
```

The runtime has no spawn `--base`/`--revision`. Each fresh Reviewer Scout receives its OWN clean native worktree, immutable base/head and standalone artifacts/handoff, then detaches reviewed-head only in that worktree and asserts it. Local branch objects must be available; missing identity blocks review. Review is fresh/read-only with tracked-source before/after assertions. Native Codex may bypass sandbox/approvals, so this is no claim of enforced OS isolation.

Return first FAIL_IMPLEMENTATION findings with `fm-send.sh <shipper-task> '<structured findings>'`. Native stopped-worker recovery is `fm-control.sh <task> relaunch --harness codex --note-file <handoff>`; it has no resume verb. One repair, NEW review2, then escalation. Preserve durable attempts/results through restart. FAIL_SPEC and SPEC_CHANGE_REQUIRED route to OpenSpec update; NEEDS_HUMAN_DECISION to the user.

After PASS, existing sync/archive can prepare lifecycle changes, but FirstMate's native guarded landing must await separate explicit user approval. No automatic merge. Do not teardown a report-owning task until its report is retained or safely captured; native teardown owns pane/worktree cleanup. Never close unrelated sessions/worktrees or delete still-owned data. See [live scenario procedures](agent-workflow-scenarios.md) and [planning schema/receipts](../planning/README.md).
