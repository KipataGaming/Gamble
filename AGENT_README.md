**DevPair Agent — Usage & Example Prompts

- **Purpose:** A focused coding assistant for the Gamble Godot C# project. Helps with code changes, documentation, refactors, and tests while respecting repo conventions.

- **Quick Start:** Address the agent with the tag `agent:devpair` or include the phrase `Agent: DevPair` in your prompt.

- **Defaults applied:**
  - Commit style: single, focused commits (squash on merge).
  - Branching: `feature/*` for new work, `fix/*` for bugfixes.
  - Formatting: run `dotnet format` before committing.
  - CI: run unit tests locally and ensure they pass before creating PRs.

- **Example prompts:**
  - "Agent: DevPair — Refactor `FarmGridBridge` to reduce duplication and add a unit test for `CalculatePlotIndex()`"
  - "Agent: DevPair — Add README docs for the Farming subsystem and link source files"
  - "Agent: DevPair — Implement save/load for `FarmPlotData` with minimal API changes; show required PR description"
  - "Agent: DevPair — Prepare a small changelist to fix null-checks in `PlayerController` and run tests"

- **Workflow suggestions:**
  1. Ask the agent for a proposed diff or patch.
  2. Review changes locally; run `dotnet format` and tests.
  3. Approve and push to `feature/*` branch and open a PR.

- **Where to add this to team tooling:**
  - Add `.agent.md` to repo root (already present).
  - Optionally add a short example prompt file under `.github/` or `.vscode/` for onboarding.

- **Contact:** Reply in this repo or open an issue named `agent:devpair` to propose further tweaks.
