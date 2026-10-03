# Scenario 3 implementation admission

Recorded at 2026-10-03T10:27:13Z from clean project HEAD `ca5bdd9f4e538d1457f86b2d35caadb5bf9ccefc`, after the approved planning landing from `eb8298a9f94c36ec2f2e7c01cbd3a5a063c7e0a7`. The [approval](approval.md) quotes the exact user answer and request. Exactly one change, `poc-verification-command-guide`, is admitted for implementation; no other implementation should be in progress.

The already-built DevRunner `planning-inputs poc-verification-command-guide` command, run from this isolated worktree, returned exit 0 with:

- `head`: `ca5bdd9f4e538d1457f86b2d35caadb5bf9ccefc`
- `spec_digest`: `7ec147b34b7293c7cd25ddaa7419ee0e586d2a7cbf1efe43c6c285479bbbb9c2`
- `code_digest`: `fd3171dff771350eb01bdbbb3cb512ee12bd33e252430fc5409c85afbc02eb70`

Canonical input hashes from that command:

| Input | SHA-256 |
| --- | --- |
| `openspec/specs/background-music/spec.md` | `d89f977b9cb41384c96a675009a1fb20544b02c108974e8fa251a018641b1f2a` |
| `openspec/specs/city-tabletop/spec.md` | `a21410ee858b9b31c773c939e1cbad1d8fbc7d4d89fe2d7fd622f002b494c5d6` |
| `openspec/specs/client-settings/spec.md` | `674e52ef413b746502e472a543e5638e035a666223ed42c3c5ebb54c4122f3af` |
| `openspec/specs/client-updates/spec.md` | `6d5e9f6a1cecd17eb1536539893299116c10fe110af05473e81a96f5c9560793` |
| `openspec/specs/combat-archetypes/spec.md` | `447f188654a865d7deebceea2e4b21b2299b7dad4fa162d380c298a8b71a729d` |
| `openspec/specs/coop-city-match/spec.md` | `791b0988b4713a95fa72262540833df8d1a642190d5ccbbcee154bb87a3104c9` |
| `openspec/specs/coop-verification/spec.md` | `f12e88c676363eb79d925a260ce626aa8b214e3769df68e68f23610626f3c058` |
| `openspec/specs/desktop-installation/spec.md` | `0f969e51b2d05a1913cbb04536c7970f62749a2cd46de150822aa35cdc1375e5` |
| `openspec/specs/ecs-unit-combat/spec.md` | `0c0ecb66598fae761d6bc0fc7c423b7b8016998229c8c692cea8e547f38ec1e0` |
| `openspec/specs/game-feedback/spec.md` | `31fba8bae3cba5fd0c19fa4fc6d098fe9e0f868f38dda4749658f5e99408e286` |
| `openspec/specs/game-launcher/spec.md` | `24ad22c7f503016ec0b0fac1f91b1c68b21c169375d7c4ae7a2a127cecde5ff0` |
| `openspec/specs/linux-test-execution/spec.md` | `0d2c7e598da92b5f43577d0685ebedba147088b0e6a6696b656ff0ee20bc4b98` |
| `openspec/specs/resumable-multiplayer/spec.md` | `6125cdacfff24ab1b220eedbc5368280493400f8a13e9d452612fba8da0ad797` |
| `openspec/specs/steam-sessions/spec.md` | `d146e6d8d708b9089a62e4d77b0bdbc891436694813d391c5afe23b4f2695bd2` |
| `openspec/specs/themed-ui/spec.md` | `879da092ef66b80c431e0e9af2ed2e196cb0abb06968f616569a7d62a423572b` |
| `openspec/specs/versioned-distribution/spec.md` | `d678fb71a17b06be01e0a10db2db4a19abb4cab362e02e95444bde7c4040ec21` |

Implementation scope is the approved README-only guide. Game code and game tests remain untouched. This bookkeeping uses no builds, restores, game tests, CI, exports or installs; implementation remains unmerged until separate approval, and no additional production trial changes are admitted.

The earlier `poc-scenario3-approval-bookkeeping` attempt is an execution anomaly only, not product evidence. It stopped with no project edits or admission because one PNG byte in its disposable worktree differed from committed bytes despite clean Git status. This fresh worktree produced the expected code digest.

With the roadmap temporarily `ready`, the already-built DevRunner `planning-next --json` exited 0 with `valid: true`, `change: poc-verification-command-guide` and no diagnostics. After setting `in_progress`, `planning-validate --json` exited 0 with `valid: true` and no diagnostics; `planning-next --json` exited 0 with `valid: true`, `change: null` and no diagnostics because an implementation is in progress. The implementation Shipper must start from the approved current proposal and preserve this single-change scope; a later requirement change requires renewed approval.
