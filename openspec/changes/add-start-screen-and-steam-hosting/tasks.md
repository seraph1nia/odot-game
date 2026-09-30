# Tasks

This is one change delivered in seven phases. Each phase lands its own tests and documentation. Check a task only when its stated evidence exists. Phases 2–4 can proceed while external Steam prerequisites are unavailable; Steam-dependent tasks in Phases 1, 5, and 7 remain unchecked until verified. A skipped real-Steam run is not a pass.

## 1. Integration prerequisites and compatibility

- [ ] 1.1 Establish the accepted `add-background-music-and-settings` baseline without recreating that feature; verify its checks pass and record the settings/menu/audio APIs and persistence behavior this change will reuse.
- [ ] 1.2 Record the selected official GodotSteam GDExtension/native peer release, canonical maintenance, source/license provenance, and observed archive hash; verify its non-prerelease metadata and publisher unstable designation are both recorded, along with the accepted development choice and outstanding release assessment. Exclude beta C# bindings, paused peer adapters, engine forks, custom engine builds, and an automatic SDK fallback.
- [ ] 1.3 Reproduce the preliminary compatibility probe with built-in C# interop or minimal GDScript glue; verify AppID 480 initialization, native class loading/casting/assignment, C# signals/callbacks, peer close/disposal, and SDK shutdown in stock Godot .NET 4.7.2 and an isolated Linux export. Investigate the initial editor-import crash and retain repeated import/shutdown results with sanitized logs rather than marking it resolved from one retry.
- [ ] 1.4 Package pinned extension/native files and licenses with ordinary Godot .NET templates, default Steam development and development exports to AppID 480 with an override, and keep initialization optional; verify isolated exports load it, offline/headless checks need no Steam, and production packaging rejects missing/development AppIDs and excludes development `steam_appid.txt`/settings.
- [ ] 1.5 Document two distinct Steam accounts, two machines, runtime/access prerequisites for AppID 480, and separate own-AppID prerequisites for real Steam cold launch; verify the development setup does not require buying/registering a production AppID and the commands/prerequisite gates are recorded in `docs/verification.md`.
- [ ] 1.6 Prove the native peer's reliable channel 0 requests/acks and channel 1 snapshots between real accounts in source/exported probes using AppID 480; verify authenticated peer-to-Steam identity mapping, command progress under snapshot load, and connection/shutdown behavior. Leave this task pending if accounts/machines or observed delivery are missing; do not replace the native peer with a custom wrapper.

Gate: the selected maintained GodotSteam GDExtension/native peer is pinned, its stability qualification remains visible, and source/export lifecycle and real-channel proof are available before Steam gameplay work starts.

## 2. Shared authority and session boundary

- [ ] 2.1 Extract an engine-independent authority/session helper around the existing `Match`, players, resume credentials, and command ledgers; add core tests proving validated local and remote bindings reject foreign-city, stale-turn, invalid-value, and unaffordable actions identically.
- [ ] 2.2 Add explicit solo, playing-host, guest, and dedicated-session policies with a bound local player instead of a loopback handshake; verify tests cover solo's single city, hosted host-only start, dedicated connected-member start, capacity four, and fresh-player refusal after start.
- [ ] 2.3 Route host-local, solo, and remote requests through one validation/acknowledgment path with existing retry protection and remote rate limiting; verify accepted-request retry is idempotent and invalid/concurrent resume claims cannot replace a valid controller.
- [ ] 2.4 Introduce the narrow snapshot/action/status session interface used by `Tabletop` and turn existing ENet RPC handlers into delivery adapters; verify existing core and dedicated ENet process tests still pass, including incompatible protocol rejection and exported reconnect behavior.
- [ ] 2.5 Keep numerical stepping in the authority's fixed physics tick and feed local presentation and remote broadcasts from authoritative snapshots; verify one authority advances each session and guest rendering never advances economy/combat, comparing state at a common revision or paused tick.
- [ ] 2.6 Extend private resume storage to distinguish transport, application, endpoint, and match while retaining compatible ENet resumes; test old-match and cross-namespace refusal and document authority ownership, message boundaries, and compatibility in the relevant developer/gameplay docs.

Gate: original rule/network regressions pass and local actions receive the same validation and retry guarantees as guest actions.

## 3. Application menu and single player

- [ ] 3.1 Add a persistent graphical application owner and native four-button start screen with static bundled medieval scenery and no match/network creation; verify pointer/keyboard navigation at 1100x820 and 1280x720 and explicit server/client role bypass, including headless absence of presentation.
- [ ] 3.2 Move the existing settings dialog and one music player to application lifetime and share their entry points across screens; verify saved preferences apply before playback, music continues across transitions, modal input is blocked, and closing settings restores valid focus without changing match readiness/pause.
- [ ] 3.3 Implement Single player through the bound local session and ordinary validated start action; verify offline process tests cover exactly one city/nine plots, normal build/recruit/ready/pause behavior, and fresh state on a second session, proving no socket, Steam login, or server child is required.
- [ ] 3.4 Add Multiplayer's Host game/Back view and recoverable platform-availability feedback; verify unavailable Steam leaves Single player, Settings, Back, and Exit usable and cannot present a local substitute as an online lobby.
- [ ] 3.5 Implement Return to menu with session generation checks, transient input/selection/request cleanup, and preserved preferences/private guest credentials; test repeated leave and delayed callbacks across two fresh sessions, including a session ending while settings are open.
- [ ] 3.6 Make Exit Game and native close share bounded cleanup and actual process termination; add graphical input/process-exit checks and document menu, solo, settings, return, and exit behavior in `docs/gameplay.md`, verifying no gameplay child or active authority remains.

Gate: a normal offline launch can enter, play, leave, and restart solo, open shared settings, and terminate through the visible controls.

## 4. Playing host and local development

- [ ] 4.1 Add explicit graphical/headless playing-host launch roles with a bound local host and ENet guest delivery while preserving existing endpoint arguments; verify a real host plus guest see the same roster and only the playing host can start the hosted match.
- [ ] 4.2 Connect hosted roster, Invite/Start availability, local action feedback, and Return to menu to the existing bottom-panel/tabletop flow; verify guest start refusal, valid host/guest purchases, direct plot selection, and layout/focus at both supported verification sizes.
- [ ] 4.3 Extend the existing `dev` runner to launch one playing host and one guest by default with a bounded option for up to three guests; isolate settings and resume storage per process and verify one-command startup, capacity enforcement, useful failure output, and cleanup of owned processes only.
- [ ] 4.4 Add real ENet playing-host process scenarios for shared purchases/readiness/combat, invalid local/remote actions, and disconnected-city retention/resume while building, paused, and eliminated; verify ordinary actions and authoritative state predicates without gameplay grants or forced outcomes.
- [ ] 4.5 Implement orderly hosted-session end and unexpected host-loss handling with disabled guest input and bounded reconnect/return feedback; test host exit and Return to menu, guest return/rejoin, and a fresh host session in the same process rejecting old credentials/callbacks without migration.
- [ ] 4.6 Update developer commands and host-lifetime documentation; verify documented server/client/dev commands and existing network failure checks still work, including missing server, occupied port, stopped server, and exported reconnect.

Gate: real local playing-host and guest processes exercise authoritative gameplay, reconnect, host end, and fresh-session isolation reproducibly without Steam.

## 5. Steam discovery and gameplay

- [ ] 5.1 Add one application-thread owner with small extension helpers for initialization, callback polling, availability, and shutdown; test initialization failure and pending-operation exit through a controllable boundary and verify local/headless launches skip Steam initialization and remain usable, without adding SDK bindings or a transport framework.
- [ ] 5.2 Create a private capacity-four lobby and native Steam overlay Invite friends action with minimal non-secret discovery metadata; verify a real host/guest invitation reaches the lobby, authoritative roster admission controls city creation, and only the original playing host starts the game.
- [ ] 5.3 Route runtime extension invite signals and `+connect_lobby`/SDK launch arguments through one deduplicated join operation; test pending/duplicate invitations and native active-session accept/decline behavior, verify real warm invitations on AppID 480 and explicit development launch-argument routing, and reserve genuine Steam cold-launch proof for the own-AppID acceptance gate without silently discarding a current match.
- [ ] 5.4 Create the extension's native host/client peer through small helpers and retain the shared handshake/command/ack/snapshot/session-ended Godot RPC handlers; verify SDK-authenticated identity lookup, protocol refusal, both reliable channels under load, ordinary host/guest actions, and relay-capable P2P without a custom Steam-to-Godot wrapper. Validate the game's lobby identifier/protocol before connecting so unrelated AppID 480 projects are refused.
- [ ] 5.5 Bind guest credentials to authenticated Steam identity and immutable original host identity, lock new-city admission after start without preventing eligible reconnect, and implement retained-city resume; test wrong-account/token and fresh-player rejection, valid paused/eliminated resumes, and rejection of a previous match at the same host.
- [ ] 5.6 Apply session-generation cleanup to Steam subscriptions, sockets, and lobby membership and preserve no-migration behavior despite automatic lobby-owner reassignment; test repeated host/join/leave and stale callbacks, verify real guest/host departure feedback, and document supported invites, reconnect, platform errors, and dependency/export setup.

Gate: GodotSteam's native peer delivers authenticated gameplay and real warm invitations between accounts with the stability qualification recorded; boundary tests, development launch arguments, or ENet substitutions cannot close remaining actual Steam checks.

## 6. Automated regression and graphical E2E

- [ ] 6.1 Extend the existing runner's bounded event predicates and failure artifacts for application screen/session roles and read-only visible-control geometry; verify diagnostics expose no credentials and UI automation uses real pointer/keyboard input rather than invoking callbacks or modifying match state.
- [ ] 6.2 Add the integrated `test-ui` route for source applications using isolated storage and a configured display; verify visible menu/settings/solo controls, direct hex/building interaction, accepted/rejected action feedback, hosted start eligibility, return/rehost, and Exit process termination with state assertions alongside screenshots.
- [ ] 6.3 Run the same representative graphical session transitions and ordinary gameplay in Linux exports outside the source tree; verify settings/music/assets load, RPC reconnect remains functional, native close terminates, and failures retain logs/screenshots within bounded deadlines.
- [ ] 6.4 Add a small drop/delay interceptor only at the shared message boundary if needed for deterministic retry/stale-delivery scenarios; verify lost acknowledgments cannot double-charge and delayed disposed-session packets cannot change a new session, while retaining real disconnect/restart tests as network evidence.
- [ ] 6.5 Integrate local core/process/export gates into existing CI without Steam accounts and expose separate `test-ui` and paired `test-steam` runner support; verify absent display/Steam prerequisites are explicitly unexecuted rather than passed, diagnostics distinguish prerequisites from failures, and teardown leaves no owned child process.
- [ ] 6.6 Document the four verification layers, exact task commands, display/account requirements, snapshot comparison strategy, and failure artifacts in `docs/verification.md`; verify commands run as documented and this phase composes the tests already landed in earlier phases.

Gate: ordinary CI stays Steam-independent, graphical E2E uses actual controls, and each external gate reports executed evidence or a clear incomplete state.

## 7. Real Steam and export acceptance

- [ ] 7.1 Run paired Linux development exports defaulting to AppID 480 (or an explicit game-AppID override) with two authorized accounts on two machines, including different NAT networks without port forwarding; verify both logs identify the same running match and supported native connection diagnostics prove an actual relay route without logging private credentials.
- [ ] 7.2 Verify runtime invitations, host-only start, shared purchases/readiness/combat, and paused guest reconnect through the paired runner/manual overlay procedure; separately verify genuine Steam cold-launch invitations using this game's own configured AppID/launch registration. Retain bounded state evidence and fresh-join refusal; leave this task incomplete if the cold-launch prerequisite is missing rather than counting explicit AppID 480 launch-argument tests as a pass.
- [ ] 7.3 Verify host Return to menu, native close, and unexpected loss across the real Steam transport, including automatic lobby-owner reassignment; confirm guests never become authority, get actionable feedback, and cannot restore old credentials into a newly hosted match.
- [ ] 7.4 Run complete local CI/export checks from a clean checkout or isolated clean build alongside real-Steam export acceptance; verify packaged native dependencies/licenses, offline solo, explicit CLI compatibility, development exports using 480, and production packaging requiring the game's own AppID without development overrides/files.
- [ ] 7.5 Record extension versions/hashes, publisher stability qualifications, repeated editor/shutdown results, machines/networks, route evidence, commands, and supported-platform limits in the final verification record and reconcile docs with observed behavior; verify every requirement has evidence and leave the change incomplete if release stability, real-Steam acceptance, or a required prerequisite remains unverified.

Gate: the full change is complete only when all local and real-Steam requirements have observed passing evidence; no save recovery, host migration, or untested platform support is implied.
