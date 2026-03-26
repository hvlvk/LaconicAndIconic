---
description: "Generate PR descriptions using the canonical orchestrator workflow."
agent: Orchestrator
argument-hint: "**Specify:** git branch/commit (optional, defaults to current) | --include-stats (show file/line counts)"
---

# PR Description Prompt

Generate comprehensive pull request description from code changes, including summary, testing, and migration notes.

## Task

1. Analyze diff between branches or commits
2. Extract feature/fix/refactor summary
3. List breaking changes and migrations needed
4. Identify testing performed and edge cases
5. Generate PR body with organized sections
6. Include checklist for code review and deployment

## Context

**Reference**: [copilot-instructions.md](../copilot-instructions.md)

**Include Sections**:
- **Summary**: What feature/bug/refactor (1-2 sentences)
- **Changes**: File listing grouped by type
- **Database**: Migration requirements (if applicable)
- **Frontend**: UI changes and testing
- **Breaking Changes**: API incompatibilities, deprecated patterns
- **Testing**: Test coverage, manual test steps
- **Deployment**: Special deployment instructions or safety notes
- **Checklist**: Pre-merge verification items

## Example

```bash
# Generate PR description for current uncommitted changes
/pr-description

# Generate for comparison to main branch
/pr-description main

# Generate with detailed statistics
/pr-description --include-stats
```

## Success Criteria

✅ PR description generated automatically
✅ All changed files listed
✅ Database/migrations noted
✅ Deployment instructions clear

