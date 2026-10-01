# Verification

## Before implementation

`mise run ci` passed at clean commit `f6ece16`: 186.46 runner seconds / 190.49 seconds including runner preparation. Evidence: `logs/20260930-210832-f426f209/`. All 80 core and 107 runner tests, six network scenarios, five source UI slices, offline native-extension probes, sequential Linux client/server exports, headless package smoke, and graphical package smoke passed. Source launcher took 20.57 seconds. Tools remain .NET 10.0.401 / Godot 4.7.2 .NET; graphical checks use owned X11, llvmpipe, and Dummy audio.

## API and result semantics

Inspected the pinned official [GodotSteam v4.22.1-gde source](https://codeberg.org/godotsteam/godotsteam/src/tag/v4.22.1-gde/godotsteam.cpp), header and enums. `getFriendCount(flags)` and `getFriendByIndex(index, flags)` take immediate-friend flags (4). `getFriendPersonaName(id)` returns a string; `getFriendPersonaState(id)` returns the Steam persona enum; `getFriendRelationship(id)` reports immediate friendship as 3. Persona states 1–6 map to online/busy/away/snooze/looking to trade/looking to play; offline/invisible/unknown map to offline. `inviteUserToLobby(lobbyId, recipientId)` returns Steam's boolean result unchanged.

[Valve's invitation API](https://partner.steamgames.com/doc/api/ISteamMatchmaking#InviteUserToLobby) reports a successful send request independently of the recipient responding. The picker reports invitation sent only for true; false/exception produces retryable failure. It does not claim delivery, acceptance, connection, or city admission. Native API enumeration and offline method-presence checks never send an invitation. No dependency or lock change is required.

## UI regression admission

Extend `launcher` with two short owned offline friends-dialog routes at its existing supported sizes. Meaningful defects: wrong duplicate-name recipient, modal gameplay leakage, unusable refresh/error recovery, focus loss on Escape/Close, and stale dialog/session cleanup. Cheap policy tests cannot establish rendered control placement or native modal input. Setup uses an existing playing-host process helper and a minimal fake friend-query/send boundary, no real Steam operation and no complete battle. Cost is two short extra owned processes and a small set of stable control selectors; actual timings will be recorded after the selected pass.

## External acceptance

Real direct invitation receipt, warm acceptance under AppID 480, and authenticated native admission require two distinct signed-in accounts on separate machines. These remain unexecuted until observed. Fixture and single-account API compatibility evidence do not satisfy task 3.2, prove relay routing, or establish production cold launch.

## Implementation checks

- `mise run test` passed after invitation-policy implementation: 89 core tests and 107 runner tests. Nine added cases cover account identity, old/reopened picker tickets, lobby/generation changes, login/friendship loss, refresh removal, reentrant activation, exception recovery, and closing during friendship lookup.
- Locked solution build passed during `mise run test-ui --scenario launcher` preparation with zero warnings/errors. Initial build feedback corrected formatting, a missing xUnit import, and a nullable assertion in the shared owned-worker guard. These were development fixes, not accepted failures.

- Selected `mise run test-ui --scenario launcher` passed in 31.63 scenario seconds / 42.00 runner seconds (`logs/20260930-212225-f004c248/`). Two friends-fixture routes use 32 rows with duplicate names and verify actual Tab scrolling to the last row, correct selected account 202, sent/rejected feedback, empty/unavailable list recovery, outside-input blocking, Escape/Close focus, consent replacement, stale session cleanup and native exit. Inspected `launcher/friends-list-1280x720.png`; source controls fit and remain readable. Approximate added launcher cost relative to the before run: 11.06 seconds for two short owned routes, with no extra battle.
- Failed UI development attempts are retained at `logs/20260930-211935-f102047d/`, `logs/20260930-212030-d5b28343/`, and `logs/20260930-212114-1ef75b40/`. Corrected explicit Steam-disabled setup for owned graphical playing-host fixtures, bounded scrolling, and minimum widths for wrapped labels so initial layout cannot push Close offscreen. No failed run is counted as acceptance.
- Task 3.2 remains unexecuted: this workspace has no second machine/account participant. No real invitation was sent during these local checks. The checked-in `--scenario direct-invite` path requires manual agreed-recipient selection, native send acceptance, actual guest callback and authenticated admission, rejects `--lobby`, and preserves ordinary shared gameplay assertions.

- `mise run check-steam-extension` passed in 14.14 runner seconds (`logs/20260930-212337-355d3457/`): actual AppID 480 initialization, native friend enumeration/names/presence/relationship queries, C# signal and peer lifecycle. No lobby or invitation was created; identities were omitted/redacted in retained logs. Overlay availability was false in this headless probe, and friend API queries still succeeded. This establishes API compatibility, not remote acceptance.

- First after-CI attempt (`logs/20260930-212534-66091d8b/`) failed in existing `authority-resume-victory` while awaiting the 64-per-second rate rejection after 80 `raw null` commands. Retained client evidence shows all 80 received "Malformed command" acknowledgments and no engine errors. The unchanged authority resets its counter at whole-second boundaries; this burst can span buckets without any individual bucket exceeding 64. The same case passed in the before baseline. No invitation or gameplay policy was changed to bypass the assertion; a selected rerun and full CI retry are required before accepting the local change.

- Selected rerun `mise run test-network --scenario authority-resume-victory` passed in 31.28 scenario seconds (`logs/20260930-212826-3e113a63/`), including actual "Command rate exceeded" responses and all ordinary cooperative/reconnect assertions. This is partial coverage; it does not replace the final full CI gate.
- Final `mise run test` passed 89 core and 107 runner tests, including the updated direct-invite option guard.

## Final local acceptance

Full `mise run ci` passed on retry with unchanged implementation: 207.51 runner seconds / 208.85 seconds including runner preparation. Evidence: `logs/20260930-212920-6880c928/ci-summary.json`, UI summaries, owned engine logs and PNGs; outer transcript: `logs/steam-friend-invites-final-ci.log`.

- Locked restore, format verification and strict solution build passed.
- All 89 core and 107 runner cases passed.
- All six network scenarios passed with two workers (60.87 seconds), including the existing rate-limit rejection.
- All five source UI slices passed (71.08 seconds including private-display startup); launcher 31.11 seconds includes both picker fixtures and preserves its original cooperative/lifecycle assertions.
- Client and server exports passed sequentially (6.91 / 3.92 seconds), followed by headless package smoke (1.09 seconds) and offline extension compatibility probes.
- Graphical exported-package smoke passed (43.31 scenario seconds / 46.61 seconds including display startup), including picker selection/lifetime checks at both sizes and all original packed-resource/gameplay assertions.

No dependency/tool lock, protocol, numerical gameplay or asset provenance was changed. New Godot C# `.uid` companions are source identifiers; `.godot`, build products, packages, runtime data and logs remain ignored. No publishing/upload/deployment occurred. Final whitespace and strict OpenSpec validation pass.

Task status: 10/11 complete; task 3.2 remains explicitly unexecuted because two machines/accounts are unavailable. This change is implemented and locally verified, but genuine direct-invitation receipt/acceptance, remote Steam gameplay/relay, native Windows runtime and production cold launch are not established by these local results. The user explicitly authorized archiving on 2026-10-01 after reviewing this remaining external check. Archiving closes this locally verified development change; task 3.2 stays unchecked and its external acceptance remains unverified.
