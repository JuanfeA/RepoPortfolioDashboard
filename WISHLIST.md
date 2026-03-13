# Repo Portfolio Dashboard - Feature Wishlist

> Prioritized backlog of features, enhancements, and ideas

## Priority Legend
- 🔴 **P0 - Critical**: Must have for MVP
- 🟠 **P1 - High**: Should have for v1.0
- 🟡 **P2 - Medium**: Nice to have
- 🟢 **P3 - Low**: Future consideration

---

## 🔴 P0 - Critical (MVP)

### Core Features
- [x] Repository sync from GitHub
- [x] Automated scoring engine
- [ ] View all repositories in dashboard
- [ ] Repository detail view with metrics
- [ ] Manual score override capability

### Data
- [x] SQLite local storage
- [ ] Initial migration + seeding
- [ ] Data persistence across sessions

### Desktop App
- [x] Basic WPF UI structure
- [ ] Repository list with sorting
- [ ] Score display with visual indicators
- [ ] Sync button functionality
- [ ] Settings for GitHub token

---

## 🟠 P1 - High (v1.0)

### Scoring Enhancements
- [ ] Custom scoring criteria editor
- [ ] Weight presets (Security-focused, Documentation-focused)
- [ ] Score history tracking
- [ ] Trend analysis (improving/declining)

### UI/UX
- [ ] Dark/Light theme toggle
- [ ] Repository filtering by language
- [ ] Sort by score, name, last updated
- [ ] Search repositories
- [ ] Card view vs list view toggle

### Integration
- [ ] GitHub Personal Access Token configuration
- [ ] Multi-account support
- [ ] Organization repository support
- [ ] GitHub Enterprise Server support

### API Layer
- [ ] REST API endpoints for headless access
- [ ] Swagger/OpenAPI documentation
- [ ] Authentication middleware

---

## 🟡 P2 - Medium (v1.x)

### Advanced Analytics
- [ ] Portfolio health score (aggregate)
- [ ] Language breakdown pie chart
- [ ] Activity heatmap
- [ ] Scoring recommendations
- [ ] Export reports (PDF, JSON, CSV)

### Notifications
- [ ] Score threshold alerts
- [ ] Stale repository warnings
- [ ] Dependency vulnerability alerts (via GitHub API)

### Performance
- [ ] Background sync scheduler
- [ ] Incremental sync (only changed repos)
- [ ] Caching layer for API responses

### Desktop Polish
- [ ] System tray integration
- [ ] Windows notifications
- [ ] Keyboard shortcuts
- [ ] Accessibility improvements

---

## 🟢 P3 - Low (Future)

### NuGet Package
- [ ] Extract Core as standalone NuGet package
- [ ] CLI tool for headless scoring
- [ ] dotnet tool integration

### Extended Integrations
- [ ] GitLab support
- [ ] Bitbucket support
- [ ] Azure DevOps support
- [ ] Jira issue integration

### AI/ML Features
- [ ] AI-powered README quality assessment
- [ ] Code complexity estimation
- [ ] Predicted maintenance effort
- [ ] Similar repository suggestions

### Collaboration
- [ ] Team sharing capabilities
- [ ] Shared scoring presets
- [ ] Comments on repositories
- [ ] Tagging/labeling system

### Monetization Ready
- [ ] License key validation system
- [ ] Feature flags for tiers
- [ ] Usage analytics (opt-in)
- [ ] Update checking

---

## Rejected / On Hold

| Feature | Reason | Date |
|---------|--------|------|
| Web UI | Desktop-first priority | 2026-03-12 |
| Real-time sync | Complexity, not needed | 2026-03-12 |
| Social features | Out of scope | 2026-03-12 |

---

## Voting / Suggestions

Submit feature requests via GitHub Issues when repository is public.

### User Requested
_No external requests yet_

---

## Notes

- Features are moved from P3 → P0 based on user feedback and technical dependency
- Each sprint should focus on 2-3 P0/P1 features max
- P2/P3 items are "someday/maybe" unless promoted
