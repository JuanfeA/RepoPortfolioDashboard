# Repo Portfolio Dashboard - Roadmap

> Strategic timeline for feature delivery

## Version Timeline

```
v0.1.0 ──────▶ v0.2.0 ──────▶ v1.0.0 ──────▶ v1.x ──────▶ v2.0
  │               │               │             │           │
  └─ MVP Demo    └─ API Layer   └─ Release    └─ Polish   └─ Extensions
    (Now)          (Week 1)       (Week 2)      (Week 3)    (Future)
```

---

## v0.1.0 - MVP Demo (Current)
**Target**: 2026-03-13

### Scope
- ✅ Clean Architecture scaffolding
- ✅ Core domain models
- ✅ Scoring engine (14 criteria)
- ✅ SQLite persistence
- ✅ GitHub API integration
- ✅ Basic WPF desktop UI
- 🔄 Demo with seed data
- 🔄 Repository listing
- 🔄 Basic sync functionality

### Success Criteria
- [ ] App launches without errors
- [ ] Can sync repositories from GitHub
- [ ] Scores displayed for each repo
- [ ] Data persists across restarts

---

## v0.2.0 - API & Settings
**Target**: 2026-03-14

### Scope
- [ ] REST API controllers
- [ ] Swagger documentation
- [ ] Settings page (GitHub token)
- [ ] Repository detail view
- [ ] Score breakdown display
- [ ] Git repository initialized

### Success Criteria
- [ ] API responds to all endpoints
- [ ] Can configure token in UI
- [ ] Detailed scores visible
- [ ] Committed to GitHub

---

## v1.0.0 - First Release
**Target**: 2026-03-15

### Scope
- [ ] Filtering and sorting
- [ ] Custom scoring presets
- [ ] Score history
- [ ] Error handling polish
- [ ] README documentation
- [ ] Release binaries

### Success Criteria
- [ ] Installable without dev tools
- [ ] No critical bugs
- [ ] <2s sync for 50 repos
- [ ] Published to GitHub Releases

---

## v1.1.0 - Analytics
**Target**: 2026-03-20

### Scope
- [ ] Portfolio health dashboard
- [ ] Charts and visualizations
- [ ] Export to CSV/JSON
- [ ] Scheduled background sync

---

## v1.2.0 - NuGet Package
**Target**: 2026-03-25

### Scope
- [ ] RepoPortfolio.Core on NuGet.org
- [ ] CLI tool package
- [ ] Documentation site

---

## v2.0.0 - Multi-Platform
**Target**: TBD

### Scope
- [ ] GitLab integration
- [ ] Bitbucket integration  
- [ ] Azure DevOps integration
- [ ] Premium features

---

## Milestones Summary

| Version | Status | Target Date | Key Deliverable |
|---------|--------|-------------|-----------------|
| v0.1.0 | 🔄 In Progress | 2026-03-13 | Working Demo |
| v0.2.0 | ⬜ Planned | 2026-03-14 | API Layer |
| v1.0.0 | ⬜ Planned | 2026-03-15 | First Release |
| v1.1.0 | ⬜ Planned | 2026-03-20 | Analytics |
| v1.2.0 | ⬜ Planned | 2026-03-25 | NuGet Package |
| v2.0.0 | ⬜ Future | TBD | Multi-Platform |

---

## Risk Register

| Risk | Impact | Likelihood | Mitigation |
|------|--------|------------|------------|
| GitHub API rate limits | High | Medium | Use token auth, caching |
| EF Core migration issues | Medium | Low | Test migrations early |
| WPF learning curve | Medium | Medium | Use CommunityToolkit |
| Scope creep | High | High | Stick to MVP first |

---

## Decision Log

| Date | Decision | Rationale |
|------|----------|-----------|
| 2026-03-12 | Desktop-first | User preference, simpler auth |
| 2026-03-12 | .NET 8 for Core | LTS, cross-platform |
| 2026-03-12 | Clean Architecture | Enables NuGet extraction |
| 2026-03-12 | SQLite | No server needed, portable |
| 2026-03-13 | Modular monolith | Simplicity over microservices |
