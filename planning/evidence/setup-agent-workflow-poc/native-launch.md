# Actual native primary and Scout launch

Date: 2026-10-03. Outcome: primary running; native Scout report collected; Scout completion/cleanup BLOCKED on a missing prerequisite. This does not establish full POC acceptance.

After the user explicitly said “you do it”, the bootstrap operator attached only the owned named Herdr session `odot-poc`, created a separate no-focus workspace, and launched the tracked `mise run agent-primary` inside its real managed shell. No fake pane environment was set; no unrelated session, pane, process or developer checkout was controlled. Startup performed its locked planning-tool restore/build/validation, without running game tests or launching Godot.

Actual primary: workspace `odot-primary` (`w2`), tab `w2:t1`, pane `w2:p1`, agent name `odot-primary`, Pi1.0.0. Primary session: `/home/bart/.local/share/firstmate/odot/state/pi-primary/2026-10-03T08-28-52-015Z_01a100e1-5dae-73a7-819f-6766da4684f7.jsonl`. FirstMate extensions, Odot overlay and five project skills loaded. `fm_watch_arm_pi` actually returned: “watcher: started Pi extension arm child 1; future ordinary re-arms are automatic”. Native branch supervision emitted status notifications. The primary is retained and focused for user interaction.

Pi used native `fm-brief.sh`/`fm-spawn.sh` with Codex/Herdr to dispatch Scout `setup-agent-workflow-poc-scout`. Actual endpoint: `odot-poc:w3:p2`, workspace `w3`, tab `w3:t2`; native Treehouse worktree `/home/bart/.treehouse/odot-game-ab1b67/1/odot-game`, clean detached head `eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7`. The Codex first-use trust prompt was accepted only for the owned POC clone, whose source was already reviewed and whose mise configuration the user had trusted. No coding preferences/model overrides were introduced.

The Scout wrote its standalone report to `/home/bart/.local/share/firstmate/odot/data/setup-agent-workflow-poc-scout/report.md`; a durable copy is `native-scout-report.md`. It found no eligible change, preserved project source, and ran no tests/builds/restores/game processes/installs. Both registered clone and leased worktree had empty tracked Git status at capture.

Initial native manual-backlog ownership was unrecognized because the primary wrote a free-form task row. It was corrected to the existing native checklist format under `## In flight`, retaining only this execution pointer and its actual IDs. Product roadmap order was not modified. Operator assistance during bootstrap included the trust prompt and pointer-format correction; fully autonomous admission/recovery has not been established.

## Blocking dependency discovered by actual execution

The earlier manual-route compatibility assumption was incomplete. `bin/fm-captain-hold.sh` at supplied FirstMate revision `e31bc6e620ca532c2e0e0b72f3fd7c0869a12270` unconditionally calls `require_tasks_axi` inside `command_complete`, even for `--none` and a manual backlog. It requires the shared version/features probe plus the captain-hold contract. `tasks-axi` is absent; native teardown refused because the completion gate had not passed. No gate was bypassed or completion fabricated.

Worker status: `blocked [at=1791017005]: [key=scout-completion-gate] Smoke report written; no eligible change. Native completion requires missing tasks-axi and metadata writes outside the Scout allowlist; stopping.` The primary can own task-local metadata writes, resolving the allowlist issue; the installed-tool prerequisite still remains. The stopped Scout's lease/endpoint is intentionally retained until native cleanup is permitted.

Upstream `docs/configuration.md` documents `npm install -g tasks-axi`; the supplied floor is0.2.6 plus required features/captain-hold support. Installation remains user-managed under Odot AGENTS.md. The operator requested that prerequisite and left actual state blocked. Task4.2/4.3 remain open; no archive/completion/Atomic or merges occurred.

Other observed limitation: native crew-state reported `unknown codex-unverified` while Herdr recognized the worker as working. State-file notifications and report delivery were observed, but stronger harness identity/automatic liveness acceptance still needs verification. The five live product scenarios and restart test are unexecuted. No game tests were run during this native launch.
