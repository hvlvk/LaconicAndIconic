---
description: 'ASP.NET Core MVC controller conventions'
applyTo: '**/Controllers/**'
---

# Controller Standards — ASP.NET Core MVC

## Core Contract
- Controllers inherit from `Controller` (not `ControllerBase` — this is an MVC app with views).
- Use constructor injection for services from BLL interfaces.
- Controllers call BLL services — never access DAL/DbContext directly.
- Annotate actions with `[HttpGet]`, `[HttpPost]`, etc. explicitly.
- Use `[ValidateAntiForgeryToken]` on all POST actions.

## Validation and Errors
- Validate via `ModelState.IsValid` before processing. Return `View(model)` on failure.
- For Identity operations, check `IdentityResult.Succeeded` and iterate `result.Errors`.
- Never expose stack traces or internal error details to users.

## Authentication
- Use `[Authorize]` attribute for protected actions.
- Cookie auth is configured globally — login path is `/Account/Login`.
- Identity uses `int` keys — never cast user IDs to `string`.

## View Return Patterns
- Return `View()` or `View(model)` for rendering.
- Return `RedirectToAction("Action", "Controller")` after successful mutations.
- Use `ViewData` for simple key-value context (e.g., `ViewData["ReturnUrl"]`).

## Scope Rules
- Keep controller actions thin — delegate business logic to BLL services.
- Follow existing naming: `AccountController`, `HomeController`, etc.
