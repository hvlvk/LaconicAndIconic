---
description: "Run a security-focused review through the single orchestrator workflow."
agent: Orchestrator
argument-hint: "**Specify:** file paths to review (comma-separated) | Optional: --focus=security|performance|architecture"
---

# Security Review Prompt

Conduct security-focused code review identifying vulnerabilities, data exposure risks, and compliance violations.

## Task

1. Analyze code for common security vulnerabilities (SQL injection, XSS, CSRF, auth bypass)
2. Check data handling for PII/sensitive information exposure
3. Verify access control and authorization
4. Review dependency versions for known CVEs
5. Validate error messages don't leak sensitive information
6. Report critical/high/medium findings with remediation

## Context

**Reference**: [backend.instructions.md](../instructions/backend.instructions.md), [api.instructions.md](../instructions/api.instructions.md)

**Focus Areas**:
- **Database**: EF Core parameterized queries (no raw SQL interpolation), authorization checks
- **Controllers**: `[Authorize]` on protected actions, `[ValidateAntiForgeryToken]` on POST actions
- **Views**: Razor auto-encoding for XSS prevention, `@Html.AntiForgeryToken()` in forms
- **Identity**: Password requirements, lockout configuration, cookie security flags
- **Dependencies**: NuGet package versions via `Directory.Packages.props`
- **Secrets**: Connection strings in User Secrets, no credentials in `appsettings.json`

## Example

```powershell
# Review specific controller for security
/security-review LaconicAndIconic.Web/Controllers/AccountController.cs

# Review all controllers
/security-review LaconicAndIconic.Web/Controllers/ --focus=security
```

## Output Format

```
SECURITY REVIEW REPORT
======================

CRITICAL ISSUES (must fix)
- [SQL_INJECTION] Line 45: Direct variable in query
- [AUTH_BYPASS] Missing role check on /admin endpoint

HIGH PRIORITY (fix soon)
- [XSS] Template escaping missing on user input display
- [DEPENDENCY] framework-package has known CVE

MEDIUM PRIORITY (plan fix)
- [LOGGING] Error traces exposed in error handler

COMPLIANT
✓ Authentication tokens validated on all protected routes
✓ CSRF tokens present in forms
```

## Success Criteria

✅ All critical/high findings documented
✅ Remediation steps identified
✅ No false positives
✅ Aligned with OWASP Top 10

