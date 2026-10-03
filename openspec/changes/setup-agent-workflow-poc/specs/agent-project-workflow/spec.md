# Spec Delta

## Purpose

Provide a single conversational project workflow with durable Git-backed planning, isolated workers, deterministic next-work selection and independent verification before completion.

## ADDED Requirements

### Requirement: Single primary and existing execution ownership
The user SHALL interact with one Pi/FirstMate primary. FirstMate SHALL own worker spawning, sessions, worktree isolation, supervision, completion/blocking detection, recovery and landing coordination, using Herdr for visible sessions and Codex CLI as the default Scout, Shipper and Reviewer harness. The project SHALL NOT introduce a replacement orchestration framework or custom communication channel. Missing prerequisites SHALL be reported as unexecuted with actionable details, without installing or upgrading user-managed tools or falling back to another runtime.

#### Scenario: Visible delegated work
- **WHEN** the primary delegates a repository investigation or approved implementation
- **THEN** FirstMate creates and supervises an isolated Codex worker observable in Herdr, and the primary collects its results without requiring the user to supervise the worker

#### Scenario: Missing Pi or isolation provider
- **WHEN** required runtime prerequisites are unavailable
- **THEN** setup names the missing prerequisites and live scenarios remain unexecuted rather than being reported as validated

### Requirement: Durable project truth and restart reconstruction
Idea consideration SHALL reside in `planning/ideas/`; proposed behavior in OpenSpec changes; current guarantees in `openspec/specs/`; approved implementation order and hard dependencies in `planning/roadmap.yaml`; project-wide architectural decisions in `docs/adr/`; actual behavior in source and tests. Durable planning, approval and verification artifacts SHALL be Git-tracked. FirstMate's backlog and runtime state SHALL track execution only and SHALL NOT replace the product roadmap. Conversation history SHALL NOT be required to reconstruct project understanding. Restart SHALL reconcile existing workers before dispatching anything new.

#### Scenario: Primary resumes without its earlier conversation
- **WHEN** the primary starts a fresh conversational session
- **THEN** it reconstructs planning and lifecycle state from repository artifacts, reconciles FirstMate-owned workers, and reports missing or inconsistent evidence instead of inventing prior approvals or duplicating work

### Requirement: Intake without premature promotion
Intake SHALL inspect root guidance, relevant source, current specifications, relevant active changes, roadmap, ideas and ADRs. It SHALL support direct discussion and a fresh Scout when investigation is valuable, identify assumptions/options/user questions, and write an idea artifact or standalone report without changing production code. Ideas SHALL use stable IDs and the statuses `inbox`, `exploring`, `shaped`, `promoted`, `parked`, or `rejected`. Unshaped ideas SHALL NOT automatically become OpenSpec changes. Reports SHALL be understandable without worker conversation history.

#### Scenario: Poorly shaped product idea
- **WHEN** the user asks about a fuzzy improvement to Odot
- **THEN** intake records the problem, desired outcome, current understanding and unanswered questions, links relevant implementation/specification evidence, and leaves production source untouched

### Requirement: Mandatory proposal preflight and coherent promotion
Before a meaningful new proposal, a fresh Codex Scout SHALL inspect canonical specs, all relevant active changes, roadmap, related ideas, ADRs and source. Its report SHALL explicitly address existing coverage, duplication, contradictions, prerequisites, downstream dependents, merging, splitting, ADR need, roadmap placement and obsolete proposals. Preparing planning artifacts SHALL be allowed without implementation approval. Successful promotion SHALL create or update complete OpenSpec planning artifacts, place the change and hard dependencies coherently in the roadmap, link the source idea in both directions, mark it promoted and pass planning validation. Unresolved product decisions SHALL remain visible to the user.

#### Scenario: Related active proposal exists
- **WHEN** preflight finds overlapping or contradictory active work
- **THEN** the report proposes an explicit merge, split, dependency or decision before creation, and the resulting artifacts do not silently redefine that active proposal

### Requirement: Deterministic roadmap validation
A checked-in command SHALL validate the versioned YAML roadmap and idea references without an LLM or live agent runtime. It SHALL reject duplicate change or idea IDs, missing referenced active/archived changes or ideas, missing dependencies/conflict references, dependency cycles, dependencies placed after their dependents, unknown states, malformed fields, more than one `in_progress` change, and active-work states on archived changes. Promoted ideas SHALL reference valid changes and coherent roadmap source links. Completion/archive records introduced by this workflow SHALL require independent verification evidence. Invalid input SHALL return nonzero diagnostics identifying the artifact and violation and SHALL prevent next-work selection.

#### Scenario: Dependencies cycle or violate order
- **WHEN** roadmap dependencies form a cycle or a dependency follows its dependent
- **THEN** validation fails with the involved change IDs and selection dispatches nothing

#### Scenario: Archived item claims readiness
- **WHEN** an archived change is marked `ready`, or two changes are marked `in_progress`
- **THEN** validation fails rather than returning either item as next work

#### Scenario: Idea promotion references missing work
- **WHEN** a promoted idea has no valid related change or disagrees with the roadmap's source idea links
- **THEN** validation reports the inconsistency and promotion cannot be reported successful

### Requirement: Stable next-work selection and explicit approval
The next-work command SHALL select the earliest eligible item in roadmap array order without changing that order. Eligibility SHALL require state `ready`, an existing OpenSpec proposal and complete implementation planning artifacts, completed/archived dependencies, no unresolved blockers/conflicts, and no other implementation in progress. Priority SHALL NOT override ordering or dependencies. Selection SHALL explain its result or why no item is eligible. A proposed change SHALL first become implementation work only after explicit user approval tied to that change and specification revision. A request to execute the next change SHALL authorize the specific eligible change resolved for that request; if selection or scope materially changes before dispatch, the primary SHALL return the changed decision to the user.

#### Scenario: Earlier item is blocked
- **WHEN** an earlier ready item has an unfinished dependency and a later item meets every eligibility condition
- **THEN** selection returns the later item and explains the earlier blocker without reordering the roadmap

#### Scenario: Another implementation exists
- **WHEN** a roadmap item or reconciled FirstMate worker indicates an implementation is already underway
- **THEN** the primary starts no second implementation and reports the existing work

### Requirement: Single-change implementation contract
A Shipper SHALL receive exactly one approved OpenSpec change in an isolated worktree and SHALL read its complete artifacts, relevant canonical specs, implementation and tests before editing. It SHALL implement only that change, add adequate tests, follow repository validation policy and update applicable task markers. It SHALL NOT invent requirements, silently edit specifications, reorder the roadmap, archive work or independently verify itself. Its standalone handoff SHALL include `STATUS`, `CHANGE`, `SUMMARY`, `FILES_CHANGED`, `TESTS_RUN`, `VALIDATION_RESULTS`, `OPEN_QUESTIONS`, `SPEC_DEVIATIONS`, `KNOWN_RISKS`, and `NEXT_ACTION`. Genuine specification or architectural problems SHALL stop affected work and return `SPEC_CHANGE_REQUIRED`.

#### Scenario: Invalid design assumption discovered
- **WHEN** implementation discovers that the specification does not define a necessary product behavior
- **THEN** the Shipper reports the affected requirement and evidence, stops that part, and the primary routes to OpenSpec update or a user decision without selecting semantics itself

### Requirement: Independent read-only review and bounded repair
A fresh Codex Reviewer with a context separate from the Shipper SHALL inspect proposal, design, deltas, tasks, canonical specs, exact implementation diff and tests. It SHALL evaluate coverage, conformance, correctness, regression risk, scope and architecture, without modifying production code. Its report SHALL include `RESULT`, `BLOCKING_FINDINGS`, `NON_BLOCKING_FINDINGS`, `REQUIREMENT_EVIDENCE`, `SPEC_DEVIATIONS`, and `RECOMMENDED_NEXT_ACTION`. Results SHALL be `PASS`, `FAIL_IMPLEMENTATION`, `FAIL_SPEC`, or `NEEDS_HUMAN_DECISION`. Evidence SHALL identify the reviewed revision and substantiate failures. One `FAIL_IMPLEMENTATION` SHALL permit one repair by the Shipper and a second fresh Reviewer. A second failure SHALL escalate to the user. `FAIL_SPEC` SHALL route to replanning; `NEEDS_HUMAN_DECISION` SHALL return to the user.

#### Scenario: Reviewer catches a missing edge case
- **WHEN** the first fresh review finds a requirement violation
- **THEN** its evidence returns to the Shipper, at most one repair is performed, and a separate fresh review evaluates the revised implementation

#### Scenario: Repair still fails
- **WHEN** the second review returns any failing result
- **THEN** the primary reports the remaining findings to the user and starts no third autonomous repair

### Requirement: Verified completion and separate landing authority
Sync/archive and roadmap completion SHALL require a current independent PASS, applicable successful validation, complete tasks and no remaining product decision. PASS SHALL apply to the reviewed code/specification inputs; material changes SHALL invalidate it. Archive SHALL be allowed automatically after these conditions are met and SHALL be clearly reported. Merging SHALL remain a separate explicit user approval, using FirstMate's guarded landing mechanisms, with no automatic merge autonomy in this POC. Unlanded work SHALL be retained. Legacy completed/archived changes SHALL be identifiable without fabricating retrospective independent review evidence.

#### Scenario: Shipper claims it is verified
- **WHEN** only Shipper assertions or stale review evidence exist
- **THEN** archive and roadmap completion are refused and independent review is required

#### Scenario: PASS precedes landing approval
- **WHEN** review passes but landing has not been approved
- **THEN** eligible lifecycle bookkeeping can be prepared on the owned branch, the primary reports readiness, and the isolated branch remains available without claiming that main contains the work

### Requirement: Real-project POC evidence and Atomic gate
Viability SHALL require executed real-project scenarios for fuzzy intake, promotion with fresh preflight, approved isolated implementation with fresh PASS, defect detection with bounded repair or escalation, and genuine specification ambiguity routed back to planning/user. Evidence SHALL include actual dispatch/session identities, worktree isolation, input/output revisions, structured reports, validation commands/results and production-write checks where applicable. Intentional defect injection SHALL be labelled and isolated; it SHALL NOT be misreported as a naturally occurring Shipper failure or merged into production. Restart reconstruction and deterministic repeated selection SHALL be demonstrated. Missing prerequisites, staged transcripts and mocked workers SHALL NOT count as live acceptance. Only after this baseline passes SHALL an Atomic follow-up experiment be prepared, comparing the same one or two changes on correctness, verification, resumability and human effort without baseline integration.

#### Scenario: Five scenarios have only scripted reports
- **WHEN** scenario evidence contains no actual supervised workers or some scenarios were unexecuted
- **THEN** the primary reports incomplete POC acceptance and does not claim viability or start Atomic evaluation

#### Scenario: Controlled regression exercises repair
- **WHEN** an intentional requirement violation is introduced in an owned POC worktree
- **THEN** a fresh Reviewer must discover it from the specification and diff, the real repair/re-review route runs, and the record identifies the fault injection and its cleanup
