# Tasks

## 1. Native invitation boundary and policy

- [x] 1.1 Record a successful before baseline from `mise run ci`, reusing previous evidence only if source/environment inputs match; record missing prerequisites as unexecuted without installing tools.
- [x] 1.2 Confirm friend enumeration, names/presence, friendship checks, and `inviteUserToLobby` signatures against pinned GodotSteam; add small typed helpers and a minimal injectable friend-query/send boundary in `src/Game`, verified by a locked build and non-sending compatibility checks where supported.
- [x] 1.3 Implement current lobby/generation and friendship/login validation plus send-result handling, with meaningful cheap coverage for duplicate-name recipient identity, stale selection, repeated activation, and rejected sends; verify with `mise run test` and record evidence.
- [x] 1.4 Document the direct-send result semantics and pinned API references in the change's verification notes; verify that an accepted request is never described as recipient acceptance or admission.

## 2. Friends picker and session integration

- [x] 2.1 Add the scrollable themed friends dialog with presence, duplicate-name disambiguation, Refresh, per-row Invite, result feedback, Close, and keyboard navigation; verify representative large/empty lists through the offline UI fixture at existing supported sizes.
- [x] 2.2 Wire the existing Invite friends action to the dialog and direct send, remove its overlay gate/deadline, and integrate modal input, focus restoration, incoming join consent, login loss, session leave/replacement, and process exit; verify stale sends are refused and accepted invitations still use the existing routing/admission path.
- [x] 2.3 Extend the existing checked-in `launcher` slice using explicit Steam-disabled fixtures and fresh observations, current selectors, actual input, and captures; verify selected-recipient feedback, modal blocking, Escape/Close focus restoration, empty/unavailable states, and teardown with `mise run test-ui --scenario launcher`, recording added runtime and setup/maintenance cost.
- [x] 2.4 Update README invitation instructions and `docs/verification.md` coverage/limitations for the picker, AppID 480 warm starts, and request-versus-admission outcomes; verify instructions agree with checked-in commands and preserve externally unverified Steam gates.

## 3. Paired Steam acceptance and integration checks

- [x] 3.1 Keep a checked-in, explicitly selected two-machine/account acceptance path in the existing Steam paired harness/documentation: host selects the agreed recipient in the picker, guest accepts with Odot already running, and both observe distinct cities and authenticated native admission. Verify the path retains ordinary cooperative gameplay assertions and never sends live invitations during ordinary CI.
- [ ] 3.2 Run the paired path under AppID 480 when two authorized accounts/machines are available; retain send result, actual invitation receipt/acceptance, matching match identity, and native admission evidence. If unavailable, record this acceptance task as unexecuted and leave it unchecked; local fixtures do not satisfy it.
- [x] 3.3 After locked restore, format changed C# with `dotnet format Odot.slnx --no-restore`, run `mise run test` for final relevant changes, and run the required after `mise run ci`; record evidence, distinguish filtered checks from full coverage, and confirm no generated artifacts, credentials, or dependency changes are tracked.
