---
name: dotnet-upgrade
description: 'Ready-to-use guide for comprehensive .NET framework upgrade analysis and execution'
---

# .NET Framework Upgrade Guide

Use this skill when upgrading projects from `.NET Framework`, `.NET Core`, or `.NET Standard` to a modern `.NET` target (e.g., `net8.0`).

## Project Discovery & Assessment

- **Project Classification**: Identify all projects in the solution and classify them by type (`.NET Framework`, `.NET Core`, `.NET Standard`). Analyze each `.csproj` for its current `TargetFramework` and SDK usage.
- **Dependency Compatibility**: Review external and internal dependencies for framework compatibility. Determine upgrade complexity based on the dependency graph depth.
- **Legacy Package Detection**: Identify legacy `packages.config` projects that need migration to `PackageReference` format.

## Upgrade Strategy & Sequencing

- **Project Upgrade Ordering**: Recommend a project upgrade order from least to most dependent components. Isolate class library upgrades before API or console app migrations.
- **Incremental Strategy**: Propose an incremental upgrade strategy with rollback checkpoints. Evaluate `.NET Upgrade Assistant` vs. manual upgrades based on project structure.
- **Progress Tracking**: Generate an upgrade checklist for tracking build, test, and deployment readiness across all projects.

## Framework Targeting & Code Adjustments

- **Target Framework Selection**: Suggest the correct `TargetFramework` for each project (e.g., `net8.0`). Review and update deprecated SDK or build configurations.
- **Code Modernization**: Identify patterns needing modernization (e.g., `WebHostBuilder` → `HostBuilder`). Suggest replacements for deprecated .NET APIs and third-party libraries.
- **Async Pattern Conversion**: Recommend converting synchronous calls to `async`/`await` where appropriate for improved performance and scalability.

## NuGet & Dependency Management

- **Package Compatibility**: Analyze outdated or incompatible NuGet packages and suggest compatible versions. Identify third-party libraries without .NET 8 support and provide migration paths.
- **Shared Dependency Strategy**: Recommend strategies for handling shared dependency upgrades across projects. Evaluate legacy packages and suggest Microsoft-supported alternatives.
- **Transitive Dependency Review**: Review transitive dependencies and potential version conflicts after upgrade. Suggest resolution strategies for dependency conflicts.

```powershell
# List outdated packages across the solution
dotnet list package --outdated

# List packages with known vulnerabilities
dotnet list package --vulnerable
```

## CI/CD & Build Pipeline Updates

- **Pipeline Configuration**: Analyze YAML build definitions for SDK version pinning and recommend updates. Review `UseDotNet@2` and `NuGetToolInstaller` tasks.
- **Build Pipeline Modernization**: Update build pipeline snippets for the new .NET target. Recommend validation builds on feature branches before merging to main.
- **CI Automation**: Identify opportunities to automate test and build verification. Ensure the CI pipeline installs the correct SDK version.

## Testing & Validation

- **Build Validation**: Propose validation checks to ensure the upgraded solution builds and runs successfully. Run automated unit and integration suites post-upgrade.
- **Service Integration Verification**: Generate validation steps to verify logging, telemetry, and service connectivity. Verify backward compatibility and runtime behavior.
- **Deployment Readiness**: Recommend UAT deployment verification steps before production rollout. Create testing scenarios for upgraded components.

```powershell
# Build the upgraded solution
dotnet build FinancialPortal.Accounting/FinancialPortal.Accounting.sln --configuration Release

# Run the full test suite after upgrade
dotnet test FinancialPortal.Accounting/FinancialPortal.Accounting.sln
```

## Breaking Change Analysis

- **API Deprecation Detection**: Identify deprecated APIs or removed namespaces between target versions. Use `.NET Upgrade Assistant` and API Analyzer for automated scanning.
- **API Replacement Strategy**: Recommend replacement APIs or libraries for known breaking areas. Review configuration changes such as `Startup.cs` → `Program.cs` refactoring.
- **Regression Testing**: Suggest regression testing scenarios focused on upgraded API endpoints or services. Create test plans for critical functionality validation.

## Version Control & Commit Strategy

- **Branching Strategy**: Recommend a branching strategy for a safe upgrade with rollback capability. Use one branch per project or logical group of projects.
- **PR Structure**: Create structured PRs named `Upgrade to .NET [Version] — [ProjectName]`. Tag PRs with breaking-change labels when applicable.
- **Code Review Focus**: Peer review should validate build success, test passage, and dependency correctness. Use upgrade checklists as PR description templates.

## Documentation & Communication

- **Upgrade Documentation**: Document each project's framework change in the PR description. Include before/after `TargetFramework` values and any removed dependencies.
- **Stakeholder Communication**: Communicate version upgrades and migration timelines to API consumers. Generate release notes summarizing upgrades and test results.
- **Progress Tracking**: Maintain an upgrade summary markdown checklist. Update it as each project completes its build, test, and deployment validation.

## Tools & Automation

- **`.NET Upgrade Assistant`** — automated project file updates, package migration, and code fixers
- **`dotnet list package --outdated`** — identify packages needing updates
- **`dotnet migrate`** — migrate legacy `project.json` solutions (pre-.NET Core 2)
- **API Analyzer** — detect deprecated API usage at compile time
- **Dependency graph** (`dotnet list package --include-transitive`) — visualize transitive dependencies before upgrading

```powershell
# Install the Upgrade Assistant global tool
dotnet tool install -g upgrade-assistant

# Analyze a project for upgrade readiness
upgrade-assistant analyze FinancialPortal.Accounting/FinancialPortal.Accounting.sln

# Run the interactive upgrade wizard
upgrade-assistant upgrade FinancialPortal.Accounting/FinancialPortal.Accounting.sln
```

## Final Validation & Delivery

- **Final Solution Validation**: Confirm the upgraded solution passes all build and test checks. Run production smoke tests and verify startup behavior.
- **Deployment Readiness Confirmation**: Generate final test results and build artifacts. Create a checklist summarizing completion across all projects (build / tests / deployment).
- **Release Documentation**: Produce a release note summarizing framework changes and CI/CD updates. Archive the upgrade checklist alongside the release.
