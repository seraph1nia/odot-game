# Actual owned filesystem setup

Date: 2026-10-03. Scope: filesystem configuration only; live runtime acceptance is UNEXECUTED.

FirstMate checkout: `/home/bart/projects/personal/firstmate`, inspected revision `e31bc6e620ca532c2e0e0b72f3fd7c0869a12270`. No upstream checkout edits.

Initialized previously absent owned home `/home/bart/.local/share/firstmate/odot` through tracked `tools/AgentWorkflow/agent-primary --init`. Installed templates: backend `herdr`, crew harness `codex`, backlog `manual`, single-profile Codex dispatch configuration. Native project registry is `odot-game [local-only]`; native `fm-project-mode.sh odot-game` actually returned `local-only off`.

NEW owned clean project clone: `/home/bart/.local/share/firstmate/odot/projects/odot-game`, local `main`, exact initial head `eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7`. This adds only checkpoint bookkeeping to independently reviewed production head `501021722a7e03fcde0cf9675db723f08a780707`. Source was bootstrap branch `work/agent-workflow-bootstrap`; provenance remote `odot-source` points to its worktree, with no `origin`. This avoids upstream native spawn refreshing leased worktrees to unrelated remote main. No developer checkout reset or merge occurred.

Ignored owned-clone `mise.local.toml` contains the four documented ODOT path/session/checkpoint values; tracked status remains clean. Native worker sessions, Treehouse leases and Herdr topology have not been created. No worker supervision is reimplemented.

User installed Pi Herdr integration; actual `herdr integration status` reports Pi current v9 and Codex current v8. Auxiliary universal upstream requirements remain missing as documented; selected manual/static-Codex/local-only route avoids their dependent operations.

The user subsequently requested no further tests because another task is running. All previously launched owned test/display/game processes had finished and cleaned up. The second review was static only. No tests/builds/runtime acceptance or launcher `--check` (which builds the planning tool) were run during this setup. Native mode lookup and file/Git inspection are the recorded static evidence.

Start later from an actual managed shell:

```sh
herdr session attach odot-poc
# Inside that shell:
cd /home/bart/.local/share/firstmate/odot/projects/odot-game
mise run agent-primary
```

Pi extension trust/login and any local mise trust remain user-managed. The launcher proves inherited native session/socket/pane/tab/workspace identity. This root Codex session is outside Herdr; it did not set fake pane variables or control a focused session. Observe later by attaching `odot-poc` in another terminal; FirstMate owns native worker dispatch and supervision.

Scenario prompts/commands and evidence expectations are in `docs/agent-workflow-scenarios.md`; operator architecture/settings/dispatch are in `docs/agent-workflow.md`. Bootstrap is blocked awaiting live acceptance, 15/26 tasks complete. Neither local checkpoint PASS nor configured files establish the five-scenario POC. No archive, product completion, Atomic integration/experiment or automatic landing occurred.
