# Scaffold verification

Verified on 2026-09-30 using Linux x86_64 (CachyOS), .NET SDK 10.0.401 and Godot 4.7.2 .NET. Graphical checks used two X11 client windows on KDE Wayland. Windows and macOS asset selection and portable process/path handling were reviewed; those hosts have not been run. The GitHub Actions workflow was reviewed locally; its Ubuntu runner has not been executed remotely.

## Automated checks

- The core suite passes all 11 cases covering speed, diagonal/oversized/non-finite input, bounds, player removal, simultaneous collection, single-point scoring, replacement coin placement and seeded deterministic state.
- The real headless network suite checks two distinct peers, movement ownership and speed, matching snapshot ticks, shared coin/score state, late joining, player removal and server shutdown. Failure cases check unavailable servers, occupied ports, missing readiness and child exit.
- The complete CI pipeline succeeds from a fresh source checkout with no `.godot`, `bin`, `obj`, logs or export caches. Source hashes remain unchanged through import and export, including dependency locks and the C# resource UID.
- Template preparation is idempotent. Corrupt cached archives and mismatched installed version markers fail clearly in isolated engine-data directories.
- Both Linux exports start and connect from their output directories. Relocated copies also work with no editor, SDK or source files on their search paths and no display. The dedicated-server export selects server role through its feature flag.

## Failure gates and cleanup

Deliberately changed assertions were tested only in a disposable source copy. A rules failure makes both `mise run test` and `ci` fail; a network assertion failure also makes `ci` fail. Neither case invokes either export. An invalid export preset fails CI. A simulated engine export error with exit status zero also fails CI and prevents the server export.

The actual `mise run dev` task starts a ready server followed by two connected graphical clients. SIGINT returns a stopped status and removes all three owned game processes. Killing one client causes a nonzero task result and cleanup of the other client and server. An occupied port fails without terminating its owner. Independently launched `mise run server` and `mise run client` connect on a non-default port; SIGTERM cleans up their children and releases that port.

## Graphical checks

WASD and arrow-key events sent to an identified client window moved its player through the production input/RPC path, with the other client observing the movement and retaining its own position. Keyboard movement reached a coin and updated the displayed score and replacement coin in both windows.

Window captures were inspected for room bounds, distinct player colors, local-player labels, shared scores and coins, late joining with an existing score, player removal, connection-failed feedback and server-disconnected feedback. Disconnected graphical clients remain open without engine errors or further gameplay input. A reconnecting client gets a new zero-score session while existing players retain their scores.

## Scenario coverage

| Capability | Verification |
| --- | --- |
| Declared tools, source layout and ignored outputs | Locked restore/build, platform archive inspection, fresh-source CI and source-hash comparison |
| Separate desktop clients and headless server | Two-window session and real headless network suite |
| Authoritative bounded movement | Core rules, sender-only RPC contract, network ownership checks and actual keyboard input |
| Coin generations and scores | Core competing-pickup assertions, network shared-state assertions and graphical pickup |
| Joining, disconnects and feedback | Network late join/removal/shutdown checks and graphical lifecycle captures |
| Bounded orchestration and diagnostics | Readiness/exit/port tests, SIGINT/SIGTERM and child-crash checks |
| Verification before exports | Deliberate rules/network failures and successful ordered CI |
| Build-only output | Workflow has `contents: read`, a job timeout, and no upload, release, publish or deployment steps |
