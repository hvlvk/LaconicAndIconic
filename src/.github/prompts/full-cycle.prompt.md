---
description: "Full development cycle: create GitHub issue → branch → plan → implement → test → fix → PR draft. Use when starting a new feature or fix from a high-level description."
agent: Orchestrator
argument-hint: "**Specify:** high-level description of the feature or fix to implement"
tools: [read, search, edit, execute, agent, todo, github/*]
---

# Full Cycle — End-to-End Development Workflow

Execute a complete development lifecycle from a high-level description: GitHub issue creation, branch setup, planning, implementation, testing, debugging, and draft PR — all in one flow.

## Inputs

The user provides a **high-level description** of what needs to be done. Example:

```
/full-cycle Add an ImageUrl field to Recipe entity so users can attach a photo
```

## Workflow

Execute these phases **sequentially**. Report progress via todo list after each phase. If any phase fails, stop and report — do not skip phases.

---

### Phase 1 — GitHub Issue

1. Use `mcp_github_create_issue` to create an issue in `hvlvk/LaconicAndIconic`:
   - **Title**: concise summary extracted from the user's description
   - **Body**: structured description with sections: Summary, Acceptance Criteria, and Technical Notes
   - Generate the body from the user's high-level description — expand it into concrete requirements
2. Note the issue number from the response (referred to as `ISSUE_NUMBER` below)

### Phase 2 — Branch & Checkout

1. Determine a branch name: `{ISSUE_NUMBER}-{slug}` where `{slug}` is a lowercase-kebab-case summary (e.g., `42-add-recipe-image-url`)
2. Run in terminal: `git checkout -b {ISSUE_NUMBER}-{slug}` to create and switch to the feature branch

### Phase 3 — Plan (Architect)

1. Delegate to the **Architect** subagent with this prompt:

   > Read the GitHub issue description below and produce PLAN.md with atomic, incremental steps.
   >
   > **Issue #{ISSUE_NUMBER}**: {issue title}
   > {issue body}

2. Verify `PLAN.md` was created at the workspace root

### Phase 4 — Draft Implementation (Senior)

1. Delegate to the **Senior** subagent with this prompt:

   > Read the PLAN.md I just created and produce IMPLEMENTATION.md with full production-ready code and tests for every step.

2. Verify `IMPLEMENTATION.md` was created at the workspace root

### Phase 5 — Implement (Middle)

1. Delegate to the **Middle** subagent step by step:

   > Implement Step 1 from IMPLEMENTATION.md.

2. After Step 1 succeeds, continue with Step 2, Step 3, etc. until all steps are applied
3. If any step fails to apply, stop and report the mismatch

### Phase 6 — Test (Tester)

1. Delegate to the **Tester** subagent:

   > Run the full test suite with `dotnet test` and report results. If new tests were added in IMPLEMENTATION.md, verify they pass.

2. If all tests pass → proceed to Phase 7
3. If tests fail → proceed to Phase 6b

#### Phase 6b — Debug & Fix (Debugger → Middle)

1. Delegate to the **Debugger** subagent with the test failure output:

   > Analyze these test failures and propose fixes: {failure output}

2. Apply the Debugger's recommended fixes (delegate to Middle if needed)
3. Re-run tests via Tester. Repeat this loop up to **3 times**
4. If tests still fail after 3 attempts, stop and report the unresolved failures

### Phase 7 — Build Verification

1. Run in terminal: `dotnet build --configuration Release` to verify the project compiles cleanly
2. If build fails, delegate to Debugger and repeat fix cycle (max 2 attempts)

### Phase 8 — Commit & Push

1. Run in terminal:
   ```
   git add -A
   git commit -m "feat(#{ISSUE_NUMBER}): {short description}"
   git push -u origin {branch-name}
   ```

### Phase 9 — Draft Pull Request

1. Use `mcp_github_create_pull_request` in `hvlvk/LaconicAndIconic`:
   - **title**: `feat(#{ISSUE_NUMBER}): {short description}`
   - **head**: `{branch-name}`
   - **base**: `main`
   - **body**: Generate a PR description with these sections:
     - **Summary**: what was done and why (reference issue #{ISSUE_NUMBER})
     - **Changes**: list of files modified/created
     - **Testing**: test results summary
   - **draft**: `true`

### Phase 10 — Summary

Report the final status:

```
## Full Cycle Complete

- Issue: #{ISSUE_NUMBER} — {title}
- Branch: {branch-name}
- PR: #{PR_NUMBER} (draft)
- Tests: ✅ All passing / ❌ {count} failures remain
- Build: ✅ Release build clean

### Files Changed
{list of files}

### Next Steps
- Review the draft PR
- Move PR from draft to ready when satisfied
```

## Rules

- **Never skip phases** — each phase depends on the previous one
- **Stop on unrecoverable failure** — if tests fail after 3 debug cycles or build fails after 2 attempts, report and stop
- Use real GitHub MCP tools for issue/PR creation — do not simulate
- Follow all project conventions from instruction files (backend, testing, database)
- Clean up `PLAN.md` and `IMPLEMENTATION.md` only if the user requests it — they serve as documentation
