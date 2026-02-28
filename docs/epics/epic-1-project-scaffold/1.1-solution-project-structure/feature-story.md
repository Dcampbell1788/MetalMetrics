# Feature 1.1 — Solution & Project Structure

**Epic:** Epic 1 — Project Scaffold & Infrastructure
**Status:** Pending
**Priority:** Critical (Foundation)
**Estimated Effort:** Small

---

## User Story

**As a** developer on the MetalMetrics team,
**I want** a well-organized .NET 8 solution with clearly separated projects,
**so that** the codebase is maintainable, testable, and follows clean architecture principles.

---

## Acceptance Criteria

- [ ] Solution file `MetalMetrics.sln` exists at the repository root
- [ ] `MetalMetrics.Web` project exists (Razor Pages — the UI host)
- [ ] `MetalMetrics.Core` project exists (domain entities, service interfaces, DTOs)
- [ ] `MetalMetrics.Infrastructure` project exists (EF Core DbContext, AI client, external integrations)
- [ ] `MetalMetrics.Tests` project exists (MSTest unit tests)
- [ ] Project references follow clean architecture (Web → Core + Infrastructure, Infrastructure → Core, Tests → all)
- [ ] `.editorconfig` is configured with team coding conventions
- [ ] `global.json` pins the .NET 8 SDK version
- [ ] Solution builds successfully with `dotnet build`
- [ ] All projects target `net8.0`

---

## Technical Notes

- Use `dotnet new` CLI commands to scaffold projects
- Razor Pages project template: `dotnet new webapp`
- Core project: `dotnet new classlib`
- Infrastructure project: `dotnet new classlib`
- Tests project: `dotnet new mstest`
- Dependency direction: Web depends on Core + Infrastructure; Infrastructure depends on Core; Tests can reference all

---

## Dependencies

- None (this is the first feature)

---

## Definition of Done

- [ ] All four projects created and referenced correctly
- [ ] Solution builds with zero errors and zero warnings
- [ ] `.editorconfig` and `global.json` committed
- [ ] Folder structure reviewed by team
