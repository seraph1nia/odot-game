---
name: project-intake
description: Investigate fuzzy Odot product ideas and write a durable idea or standalone Scout report before any proposal or implementation.
---

Read AGENTS.md, planning/README.md, roadmap, existing ideas, relevant source/tests, canonical specs, relevant active changes and ADRs. Optionally research authoritative external documentation, distinguishing observed behavior, proposals and assumptions. Understand the problem before choosing a solution: alternatives, architectural implications, related/duplicate work, missing decisions and promotion criteria.

The primary may discuss directly when repository investigation adds little. Otherwise FirstMate dispatches a fresh Codex Scout with a scoped brief. Its deliverable is a standalone report with problem, desired outcome, current behavior/evidence, explored approaches/trade-offs, assumptions, user questions/options, decisions still needed, related ideas/changes and recommended next action. Include file/spec links another context can follow.

Persist/update an idea using planning/templates/idea.md and stable ID/status. Native Scout reports live at FM_HOME/data/<task>/report.md. The primary collects the report and delegates durable idea/report capture to a planning-only logical Scout using the native local-only Ship contract; explicitly allow only planning paths. Do not promote that Scout into a production Shipper. Landing even planning files requires separate explicit approval. Preserve unresolved product choices.

Never implement production source, create implementation approval, silently promote an idea, invent unresolved semantics or replace the roadmap with runtime tasks. Unshaped ideas remain ideas.

Example: “make it easier to tell which verification command I need” leads to README/AGENTS/docs/verification/Linux verification inspection and options for a concise command guide versus tooling; ask which confusion matters and retain it as exploring. A question about research/status effects overlapping add-city-research-and-status-effects links that active proposal and identifies uncovered problems before suggesting another change.
