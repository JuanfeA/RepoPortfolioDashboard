# Repo Portfolio Dashboard — Project Manifest

## 🎯 Purpose
Provide a unified dashboard to review all GitHub repositories in one place, with:
- Overview of repo metadata and activity.
- Scoring rubric for maturity, quality, and scope.
- Collaborative management of criteria and parameters.
- Continuous updates via GitHub API integration.

## 🏗️ Infrastructure & Tech Stack
- **Database**: PostgreSQL or Azure SQL (store repo metadata, scoring parameters, logs).
- **Integration Layer**: GitHub GraphQL/REST API, scheduled sync jobs (Azure Functions/WebJobs).
- **Logic Layer**: Scoring engine applying rubric rules, configurable weights stored in DB.
- **Presentation Layer**: Backend API + UI dashboard (React/Blazor).
- **Collaboration Layer**: Role-based access, audit logs, notifications (Slack/Teams).
- **Optional MCP**: Local agents can query DB/API to propose actions or trigger sync jobs.

## 📊 Scoring Rubric (Initial Draft)
- **Activity Score**: commits, PRs, issues (weight: 0.4).
- **Quality Score**: tests, docs, CI/CD status (weight: 0.3).
- **Maturity Score**: prototype → production classification (weight: 0.2).
- **Risk Indicators**: outdated dependencies, failing builds, inactivity > 90 days (weight: 0.1).

## 🔄 Workflow
1. Nightly sync job pulls repo metadata from GitHub API.
2. Scoring engine applies rubric → updates DB.
3. UI dashboard shows portfolio overview + repo detail.
4. Admin adjusts scoring weights or adds criteria via UI.
5. Agents (via MCP) query DB → propose actions (archive, refactor, prioritize).
6. Notifications alert when thresholds are crossed (e.g., inactivity > 180 days).

## 📂 Data Model (Draft)
- **Repos**: id, name, description, tags, last_commit, last_release.
- **Criteria**: id, name, weight, description.
- **Scores**: repo_id, criteria_id, value, timestamp.
- **Activity Logs**: repo_id, event_type, timestamp, status.

## 🚦 Criticality Hierarchy
- **Tier 1 (Mission Critical)**: Auth/security, management dashboard, syncing, error handling, audit logs.
- **Tier 2 (Operational)**: Reports, imports, data mapping, scalability.
- **Tier 3 (Enhancements)**: UX polish, sandbox support, notifications.
- **Tier 4 (Strategic)**: Extensibility, event-driven architecture, advanced analytics.

## 📌 Notes
- This manifest is a **guideline document**, not implementation code.
- Copilot should use this as context for generating scaffolds, queries, or UI components.
- Criteria and weights are adjustable; DB schema should support versioning.
