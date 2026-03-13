# Repo Portfolio Dashboard — Research & Analysis

## Executive Summary

This document provides a comprehensive analysis of the Repo Portfolio Dashboard project, evaluating tech stack options, architectural considerations, and implementation recommendations to guide decision-making before development begins.

> **Update**: For desktop-first architecture with reusable internals, see [ARCHITECTURE_DESKTOP.md](ARCHITECTURE_DESKTOP.md)

---

## 💡 Key Insight: You Don't Have to Separate UI and Backend

Modern C# development **does not require** separating frontend and backend into different services. That pattern exists for:
- Web apps needing independent scaling
- Teams working on frontend/backend separately
- Microservices architectures

For a **Windows desktop app**, a **modular monolith** is perfectly valid:
- Single executable deployment
- Shared code without HTTP overhead
- Simpler debugging and testing
- Can still expose API later using same core

The key is **clean layering** (not physical separation):
```
Desktop App → Application Layer → Core → Infrastructure
     ↑                                        ↑
     └──── Both reference same Core library ──┘
Optional API →
```

---

## 📊 Tech Stack Comparison

### 1. Database Layer

| Criteria | PostgreSQL | Azure SQL |
|----------|------------|-----------|
| **Cost** | Free/open-source, self-hosted or managed (Supabase, AWS RDS, Azure Flexible Server) | Pay-per-use, higher baseline cost |
| **JSON Support** | Native JSONB with indexing — excellent for flexible repo metadata | JSON support available but less mature |
| **Scalability** | Excellent horizontal scaling with read replicas | Built-in auto-scaling, elastic pools |
| **GitHub Integration** | Works well with any stack | Native Azure ecosystem integration |
| **Versioning/Audit** | Requires manual implementation or extensions (temporal tables via extension) | Built-in temporal tables for versioning |
| **DevOps** | More setup required | Seamless Azure DevOps integration |

**Recommendation**: 
- **PostgreSQL** — Best for flexibility, cost, and JSON handling for repo metadata
- Choose **Azure SQL** only if you're committed to Azure ecosystem and need enterprise features out-of-box

---

### 2. Integration Layer (Sync Jobs)

| Criteria | Azure Functions | Azure WebJobs | GitHub Actions | AWS Lambda |
|----------|-----------------|---------------|----------------|------------|
| **Trigger Options** | Timer, HTTP, Queue, Event Grid | Timer, Queue | Scheduled (cron), webhook | Timer, API Gateway, EventBridge |
| **Cold Start** | ~1-3s (Consumption), none (Premium) | None (Always On) | N/A (runs on schedule) | ~1-5s |
| **Cost Model** | Pay-per-execution | Part of App Service plan | Free for public repos (limited minutes) | Pay-per-execution |
| **GitHub API Integration** | Custom code required | Custom code required | Native integration | Custom code required |
| **Complexity** | Low-Medium | Low | Very Low | Low-Medium |

**Recommendation**:
- **Azure Functions (Timer Trigger)** — Best balance of simplicity, scalability, and Azure integration
- **GitHub Actions** — Consider as secondary for repo-specific triggers (webhooks on push/PR)

---

### 3. Logic Layer (Scoring Engine)

| Approach | Pros | Cons |
|----------|------|------|
| **In-Database (Stored Procedures/Views)** | Fast, atomic updates, single source of truth | Harder to test, less flexible |
| **Application Layer (C#/.NET)** | Testable, strongly-typed, easy to extend | Additional service to maintain |
| **Application Layer (Node.js/TypeScript)** | Fast development, same language as React frontend | Less type safety (unless strict TS) |
| **Hybrid** | Best of both: DB handles aggregation, app applies weights | More complexity |

**Recommendation**:
- **Application Layer** approach with configurable weights stored in DB
- Use **C#/.NET** if choosing Blazor, **TypeScript** if choosing React

---

### 4. Presentation Layer

| Criteria | React + Node API | Blazor (Server/WASM) + .NET API | Next.js (Full-stack) |
|----------|------------------|----------------------------------|----------------------|
| **Learning Curve** | Moderate (if familiar with JS/TS) | Low (if .NET background) | Moderate |
| **Performance** | Excellent | Good (Server), Excellent (WASM) | Excellent (SSR + CSR) |
| **Ecosystem** | Massive, mature | Growing, Microsoft-supported | Large, React-based |
| **Real-time Updates** | Requires Socket.io/SignalR setup | Built-in SignalR (Server) | Requires additional setup |
| **Dashboard Libraries** | Recharts, Chart.js, Tremor, Shadcn | Blazorise, MudBlazor, Syncfusion | Same as React |
| **Azure Integration** | Good (any backend) | Excellent (native .NET) | Good |

**Recommendation**:
- **React + .NET API** — Best of both worlds: rich frontend ecosystem + robust backend
- **Blazor Server** — Choose if team is .NET-focused and real-time is critical
- **Next.js** — Choose for rapid full-stack development with API routes

---

### 5. Collaboration Layer

| Feature | Implementation Options |
|---------|------------------------|
| **Role-Based Access** | ASP.NET Core Identity, Auth0, Azure AD B2C |
| **Audit Logs** | Database table with triggers, Serilog + Seq, Application Insights |
| **Notifications** | Azure Logic Apps, direct Slack/Teams webhooks, SendGrid for email |

**Recommendation**: 
- **Azure AD B2C** or **Auth0** for enterprise-grade auth
- **Application Insights** + custom audit table for logging
- **Azure Logic Apps** for notification orchestration (low-code)

---

## 🏛️ Architecture Recommendations

### Option A: Full Azure Stack (Enterprise)
```
┌─────────────────────────────────────────────────────────────────┐
│                        Azure Front Door                         │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────────┐   │
│  │ Blazor WASM │────▶│ .NET API    │────▶│ Azure SQL       │   │
│  │ (Static)    │     │ (App Service)│    │ (Managed)       │   │
│  └─────────────┘     └──────┬──────┘     └─────────────────┘   │
│                             │                                   │
│                      ┌──────▼──────┐                           │
│                      │ Azure Funcs │◀──── Timer/Webhook        │
│                      │ (Sync Jobs) │                           │
│                      └──────┬──────┘                           │
│                             │                                   │
│                      ┌──────▼──────┐                           │
│                      │ GitHub API  │                           │
│                      └─────────────┘                           │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**: Integrated, enterprise features, single cloud provider  
**Cons**: Higher cost, vendor lock-in

### Option B: Hybrid Modern Stack (Recommended)
```
┌─────────────────────────────────────────────────────────────────┐
│                        Vercel / Azure Static                    │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────────┐   │
│  │ React/Next  │────▶│ .NET API    │────▶│ PostgreSQL      │   │
│  │ (Vercel)    │     │ (Container) │     │ (Supabase/Azure)│   │
│  └─────────────┘     └──────┬──────┘     └─────────────────┘   │
│                             │                                   │
│                      ┌──────▼──────┐                           │
│                      │ Azure Funcs │◀──── Timer Trigger        │
│                      │ (Sync Jobs) │                           │
│                      └──────┬──────┘                           │
│                             │                                   │
│                      ┌──────▼──────┐                           │
│                      │ GitHub API  │                           │
│                      └─────────────┘                           │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**: Cost-effective, flexible, best tooling for each layer  
**Cons**: More integration points to manage

### Option C: Lightweight / Solo Developer
```
┌─────────────────────────────────────────────────────────────────┐
│                        Vercel                                   │
├─────────────────────────────────────────────────────────────────┤
│  ┌───────────────────────────────┐     ┌─────────────────┐     │
│  │ Next.js (Full-stack)          │────▶│ Supabase        │     │
│  │ - UI Dashboard                │     │ (PostgreSQL +   │     │
│  │ - API Routes                  │     │  Auth + Realtime│     │
│  │ - Cron Jobs (Vercel Cron)     │     └─────────────────┘     │
│  └───────────────────────────────┘                              │
│                  │                                              │
│           ┌──────▼──────┐                                      │
│           │ GitHub API  │                                      │
│           └─────────────┘                                      │
└─────────────────────────────────────────────────────────────────┘
```

**Pros**: Minimal infrastructure, fast to ship, low cost  
**Cons**: Limited scalability, less separation of concerns

---

## 📈 GitHub API Considerations

### Rate Limits
| API Type | Limit | Reset |
|----------|-------|-------|
| REST (authenticated) | 5,000 requests/hour | Rolling window |
| GraphQL (authenticated) | 5,000 points/hour | Rolling window |
| REST (unauthenticated) | 60 requests/hour | Rolling window |

### Recommended Approach
1. **Use GraphQL** — Fetches multiple fields in one request, reduces API calls
2. **Implement caching** — Store results for 6-24 hours to reduce API load
3. **Use conditional requests** — ETags for "not modified" responses (free)
4. **Consider GitHub App** — Higher rate limits, org-level access

### Key Endpoints for Scoring
```graphql
# Example GraphQL query for repo health
query($owner: String!, $name: String!) {
  repository(owner: $owner, name: $name) {
    name
    description
    pushedAt
    defaultBranchRef {
      target {
        ... on Commit {
          history(first: 1) {
            totalCount
          }
        }
      }
    }
    releases(last: 1) {
      nodes { tagName publishedAt }
    }
    issues(states: OPEN) { totalCount }
    pullRequests(states: OPEN) { totalCount }
    vulnerabilityAlerts { totalCount }
    hasWikiEnabled
    hasIssuesEnabled
  }
}
```

---

## 🧮 Scoring Engine Design

### Configurable Weights Schema
```sql
CREATE TABLE scoring_criteria (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    category VARCHAR(50) NOT NULL, -- 'activity', 'quality', 'maturity', 'risk'
    weight DECIMAL(3,2) NOT NULL CHECK (weight >= 0 AND weight <= 1),
    description TEXT,
    calculation_type VARCHAR(50), -- 'count', 'boolean', 'days_since', 'percentage'
    thresholds JSONB, -- {"low": 0, "medium": 50, "high": 100}
    is_active BOOLEAN DEFAULT true,
    version INT DEFAULT 1,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Example criteria
INSERT INTO scoring_criteria (name, category, weight, calculation_type, thresholds) VALUES
('commit_frequency', 'activity', 0.15, 'count', '{"period_days": 30, "low": 0, "medium": 5, "high": 20}'),
('open_pr_count', 'activity', 0.10, 'count', '{"low": 0, "medium": 3, "high": 10}'),
('has_ci_cd', 'quality', 0.15, 'boolean', '{}'),
('test_coverage', 'quality', 0.10, 'percentage', '{"low": 0, "medium": 60, "high": 80}'),
('days_since_release', 'maturity', 0.10, 'days_since', '{"good": 90, "warning": 180, "critical": 365}'),
('dependency_vulnerabilities', 'risk', 0.05, 'count', '{"good": 0, "warning": 3, "critical": 10}');
```

### Versioning Support
```sql
CREATE TABLE scoring_criteria_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    criteria_id UUID REFERENCES scoring_criteria(id),
    old_values JSONB,
    new_values JSONB,
    changed_by UUID,
    changed_at TIMESTAMP DEFAULT NOW()
);
```

---

## 🔒 Security Considerations

### Minimum Requirements (Tier 1 - Mission Critical)
1. **GitHub Token Storage** — Azure Key Vault or environment secrets (never in DB/code)
2. **API Authentication** — JWT with short expiration, refresh tokens
3. **Rate Limiting** — Prevent abuse of dashboard API
4. **Input Validation** — Sanitize all repo names/URLs
5. **Audit Logging** — Log all admin actions (weight changes, manual overrides)

### Data Privacy
- Store only public repo metadata OR require explicit org consent for private repos
- Implement data retention policies (e.g., delete activity > 2 years)
- GDPR consideration: Allow users to request data deletion

---

## 💰 Cost Estimation (Monthly)

### Option A: Full Azure (Enterprise)
| Service | Estimated Cost |
|---------|----------------|
| Azure SQL (Basic) | $5-15 |
| App Service (B1) | $13-55 |
| Azure Functions (Consumption) | $0-5 |
| Azure AD B2C | $0-50 (based on MAU) |
| **Total** | **$18-125/month** |

### Option B: Hybrid Stack
| Service | Estimated Cost |
|---------|----------------|
| Supabase (Pro) | $25 |
| Azure Container Apps | $0-20 |
| Azure Functions | $0-5 |
| Vercel (Pro) | $20 |
| **Total** | **$45-70/month** |

### Option C: Lightweight
| Service | Estimated Cost |
|---------|----------------|
| Supabase (Free/Pro) | $0-25 |
| Vercel (Hobby/Pro) | $0-20 |
| **Total** | **$0-45/month** |

---

## 🚀 Implementation Phases

### Phase 1: Foundation (2-3 weeks)
- [ ] Set up database schema (repos, criteria, scores)
- [ ] Implement GitHub API sync (basic metadata)
- [ ] Create simple scoring engine with hardcoded weights
- [ ] Build minimal dashboard (list view)

### Phase 2: Core Features (3-4 weeks)
- [ ] Add configurable scoring criteria UI
- [ ] Implement full scoring rubric
- [ ] Add repo detail view with historical scores
- [ ] Set up authentication

### Phase 3: Collaboration (2-3 weeks)
- [ ] Role-based access control
- [ ] Audit logging
- [ ] Notification system (thresholds)

### Phase 4: Polish (2-3 weeks)
- [ ] Dashboard visualizations (charts, trends)
- [ ] Bulk actions (archive, tag)
- [ ] MCP integration for AI agents
- [ ] Performance optimization

---

## 🎯 Decision Matrix

| If you need... | Choose... |
|----------------|-----------|
| **Windows desktop app (local-first)** | **WPF + SQLite + Clean Architecture** |
| **Desktop + optional cloud sync** | WPF + SQLite locally + PostgreSQL cloud |
| Fastest time to MVP (web) | Next.js + Supabase |
| Best .NET integration (web) | Blazor + Azure SQL |
| Rich frontend + robust backend | React + .NET API + PostgreSQL |
| Lowest cost | Supabase (free tier) + Vercel |
| Enterprise features | Full Azure stack |
| AI/MCP integration focus | PostgreSQL (flexible JSONB) + Python/Node |
| **Reusable core for monetization** | **Clean Architecture with Core library** |

---

## 📋 Next Steps

### If Going Desktop-First (Recommended)
1. **Create solution** with Clean Architecture layers (Core, Application, Infrastructure, Desktop)
2. **Implement Core** — domain models + scoring engine (no dependencies, testable)
3. **Add Infrastructure** — SQLite with EF Core, Octokit for GitHub
4. **Build WPF dashboard** — basic list view, sync button
5. **Later**: Add API project that consumes same Core/Application layers

### If Going Web-First
1. **Decide on primary tech stack** based on team skills and priorities
2. **Set up project scaffolding** with chosen tools
3. **Create GitHub App** for API access with higher rate limits
4. **Define initial 5-10 scoring criteria** with weights
5. **Prototype sync job** with 2-3 repos first

---

## 📚 References

- [GitHub GraphQL API Explorer](https://docs.github.com/en/graphql/overview/explorer)
- [GitHub REST API Rate Limits](https://docs.github.com/en/rest/rate-limit)
- [Supabase Documentation](https://supabase.com/docs)
- [Azure Functions Timer Triggers](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer)
- [MudBlazor Components](https://mudblazor.com/)
- [Tremor React Components](https://www.tremor.so/)
