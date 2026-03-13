# Repo Portfolio Dashboard — LLM-Powered Health Analysis

## Vision Statement

Transform the Repo Portfolio Dashboard from a **passive scoring tool** into an **active SDLC intelligence system** that:
- Uses LLM analysis to understand repo context, code quality, and development stage
- Provides actionable insights tied to project management practices
- Enables repos to self-report status through standardized mechanisms
- Automates health monitoring via CI/CD integration and MCP protocols

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Repo Portfolio Dashboard                            │
│                        (Central Intelligence Hub)                            │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
         ┌──────────────────────────┼──────────────────────────┐
         │                          │                          │
         ▼                          ▼                          ▼
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│   Pull Mode     │      │   Push Mode     │      │   LLM Analysis  │
│  (GitHub API)   │      │  (MCP/Webhooks) │      │   (AI Insights) │
└─────────────────┘      └─────────────────┘      └─────────────────┘
         │                          │                          │
         │ Rate-limited sync        │ Real-time updates        │ Deep analysis
         │ Metadata + stats         │ Standardized reports     │ Code review
         │                          │                          │ SDLC mapping
         ▼                          ▼                          ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Unified Data Store                              │
│           (Repositories + Scores + Health + SDLC State + AI Insights)       │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Output Channels                                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │   Desktop    │  │   REST API   │  │   Webhooks   │  │   Reports    │    │
│  │   Dashboard  │  │   (Query)    │  │  (Alerts)    │  │   (PDF/CSV)  │    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Feature Analysis: Efficiency Matrix

### 1. Data Collection Methods

| Method | Efficiency | Real-time | Setup Cost | Maintenance | Best For |
|--------|------------|-----------|------------|-------------|----------|
| **GitHub API Polling** | Medium | No (5min delay) | Low | Low | Initial sync, bulk updates |
| **GitHub Webhooks** | High | Yes | Medium | Medium | Event-driven updates |
| **MCP Server in Repos** | Very High | Yes | High | High | Rich context, bidirectional |
| **CI/CD Status Report** | High | Near real-time | Medium | Low | Build/test metrics |
| **Git Hooks** | Very High | Immediate | Medium | Low | Commit-level tracking |
| **LLM File Analysis** | Low (slow) | No | Low | Low | Deep code understanding |

### 2. Recommended Hybrid Approach

```
Priority 1: GitHub API (baseline - already implemented)
Priority 2: CI/CD Integration via GitHub Actions (push status to API)
Priority 3: LLM Analysis (on-demand, cached results)
Priority 4: MCP Server (for advanced bidirectional communication)
```

---

## Integration Options: Detailed Analysis

### Option A: GitHub Actions Status Reporter

**Mechanism**: Repos include a workflow that reports to our API after each CI run.

```yaml
# .github/workflows/portfolio-report.yml
name: Report to Portfolio Dashboard

on:
  push:
    branches: [main, master]
  pull_request:
  workflow_run:
    workflows: ["CI", "Tests"]
    types: [completed]

jobs:
  report:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Gather Metrics
        id: metrics
        run: |
          echo "commit_sha=${{ github.sha }}" >> $GITHUB_OUTPUT
          echo "branch=${{ github.ref_name }}" >> $GITHUB_OUTPUT
          echo "event=${{ github.event_name }}" >> $GITHUB_OUTPUT
          # Count files, LOC, etc.
          
      - name: Report to Portfolio API
        run: |
          curl -X POST "${{ secrets.PORTFOLIO_API_URL }}/api/repos/report" \
            -H "Authorization: Bearer ${{ secrets.PORTFOLIO_TOKEN }}" \
            -H "Content-Type: application/json" \
            -d '{
              "repo": "${{ github.repository }}",
              "sha": "${{ steps.metrics.outputs.commit_sha }}",
              "branch": "${{ steps.metrics.outputs.branch }}",
              "event": "${{ steps.metrics.outputs.event }}",
              "status": "${{ job.status }}",
              "timestamp": "${{ github.event.head_commit.timestamp }}"
            }'
```

**Efficiency**: ⭐⭐⭐⭐ (4/5)
- No polling needed
- Runs only on actual changes
- Lightweight payload
- Works with any repo that adopts the workflow

---

### Option B: MCP Server Integration

**Mechanism**: Each repo has an MCP server that exposes project state to Copilot/AI agents.

```typescript
// mcp-portfolio-reporter/src/index.ts
import { Server } from "@modelcontextprotocol/sdk/server/index.js";

const server = new Server({
  name: "portfolio-reporter",
  version: "1.0.0"
}, {
  capabilities: {
    tools: {},
    resources: {}
  }
});

// Expose project status as a resource
server.setRequestHandler("resources/read", async (request) => {
  if (request.params.uri === "project://status") {
    return {
      contents: [{
        uri: "project://status",
        mimeType: "application/json",
        text: JSON.stringify({
          phase: detectPhase(),
          maturity: calculateMaturity(),
          recentCommits: getRecentCommits(),
          openIssues: getOpenIssues(),
          testCoverage: getTestCoverage()
        })
      }]
    };
  }
});

// Tool for AI to report insights
server.setRequestHandler("tools/call", async (request) => {
  if (request.params.name === "report_health") {
    await sendToPortfolioDashboard(request.params.arguments);
    return { content: [{ type: "text", text: "Health reported successfully" }] };
  }
});
```

**Efficiency**: ⭐⭐⭐⭐⭐ (5/5 for AI integration)
- Bidirectional communication
- AI can query AND update
- Rich context available
- Complex setup per repo

---

### Option C: Standardized PROGRESS.md Parser

**Mechanism**: Repos maintain a `PROGRESS.md` file following our standard. The dashboard parses it.

```markdown
<!-- PORTFOLIO-MACHINE-READABLE -->
---
phase: DEV
maturity: L2
sprint: 3/8
health: 65
blockers: 2
---
<!-- /PORTFOLIO-MACHINE-READABLE -->

# Project Progress
... human-readable content ...
```

**Efficiency**: ⭐⭐⭐ (3/5)
- No infrastructure needed
- Works with any repo
- Requires manual updates
- Can be automated via pre-commit hook

---

### Option D: LLM-Powered Deep Analysis

**Mechanism**: Dashboard uses LLM to analyze repo contents and infer status.

```csharp
public class LlmRepoAnalyzer
{
    private readonly ILlmClient _llm;
    
    public async Task<RepoInsights> AnalyzeAsync(Repository repo)
    {
        // Fetch key files
        var readme = await _github.GetFileContent(repo.FullName, "README.md");
        var packageJson = await _github.GetFileContent(repo.FullName, "package.json");
        var csproj = await _github.GetFileContent(repo.FullName, "*.csproj");
        var changelog = await _github.GetFileContent(repo.FullName, "CHANGELOG.md");
        var recentCommits = await _github.GetCommits(repo.FullName, limit: 20);
        
        var prompt = $"""
            Analyze this repository and determine:
            1. Current SDLC phase (Ideation/Planning/Development/Testing/Release/Maintenance)
            2. Maturity level (0-100 based on: docs, tests, CI/CD, code quality)
            3. Active development areas (what features are being worked on)
            4. Technical debt indicators
            5. Risk factors
            6. Recommended next actions
            
            README:
            {readme}
            
            Package/Project File:
            {packageJson ?? csproj}
            
            Recent Commits:
            {string.Join("\n", recentCommits.Select(c => $"- {c.Message}"))}
            
            Respond in JSON format matching our RepoInsights schema.
            """;
        
        return await _llm.AnalyzeAsync<RepoInsights>(prompt);
    }
}
```

**Efficiency**: ⭐⭐ (2/5 for speed, ⭐⭐⭐⭐⭐ for insight depth)
- Very rich analysis
- Expensive API calls
- Should be cached (run daily/weekly)
- Can understand context humans define

---

## LLM Analysis Categories

### What LLM Can Infer from Repos

| Category | Source Files | Insights |
|----------|-------------|----------|
| **SDLC Phase** | README, commits, issues | Is this planning, active dev, or maintenance? |
| **Tech Stack** | package.json, *.csproj, requirements.txt | Dependencies, versions, vulnerabilities |
| **Code Quality** | Source files sample | Patterns, anti-patterns, complexity |
| **Documentation** | README, docs/, wiki | Completeness, accuracy, clarity |
| **Test Health** | Test files, coverage reports | Coverage gaps, test quality |
| **Architecture** | Folder structure, imports | Clean architecture adherence |
| **Security** | .env.example, auth code | Credential handling, vulnerabilities |
| **Activity Pattern** | Commits over time | Sprint velocity, consistency |

---

## Implementation Roadmap

### Phase 1: Enhanced GitHub Integration (Week 1)
- [ ] Implement webhook receiver endpoint
- [ ] Add GitHub Actions report endpoint
- [ ] Create workflow template for repos to adopt
- [ ] Test with 2-3 repos

### Phase 2: LLM Analysis Engine (Week 2)
- [ ] Add OpenAI/Anthropic client to Infrastructure
- [ ] Create `LlmRepoAnalyzer` service
- [ ] Implement caching layer (analysis per repo per day)
- [ ] Add "Analyze" button to dashboard
- [ ] Map LLM insights to scoring criteria

### Phase 3: Standardization Kit (Week 3)
- [ ] Create PM skill file for repos
- [ ] Create `PROGRESS.md` parser
- [ ] Build GitHub Action template package
- [ ] Create MCP server template
- [ ] Documentation for repo onboarding

### Phase 4: Automation & Alerts (Week 4)
- [ ] Health threshold alerts
- [ ] Stale repo detection
- [ ] PR/Issue integration
- [ ] Scheduled health reports

---

## Scope Analysis: What Our Token Can Do

With `repo` scope on GitHub token:

| Action | Possible | Risk Level |
|--------|----------|------------|
| Read repo metadata | ✅ | None |
| Read file contents | ✅ | None |
| Read commits/branches | ✅ | None |
| Read issues/PRs | ✅ | None |
| Create issues | ✅ | Low |
| Create PRs | ✅ | Medium |
| Create/update files | ✅ | Medium |
| Trigger workflows | ✅ | Low |
| Create webhooks | ✅ | Low |
| Delete branches | ✅ | High |

**Recommended Auto-Actions**:
1. Create issues for health alerts (e.g., "Test coverage dropped below 60%")
2. Add `PROGRESS.md` template to repos missing it
3. Create PR to add portfolio reporting workflow
4. Update README badges with health scores

---

## Efficiency Comparison Summary

| Feature | Effort | Value | Priority |
|---------|--------|-------|----------|
| GitHub API Enhancement | Low | High | P0 |
| CI/CD Workflow Reporter | Medium | High | P1 |
| LLM Deep Analysis | Medium | Very High | P1 |
| PROGRESS.md Parser | Low | Medium | P2 |
| MCP Server Template | High | High | P3 |
| Auto-Issue Creation | Low | Medium | P2 |
| Auto-PR for Setup | Medium | Medium | P3 |

---

## Next Steps

1. **Immediate**: Enhance demo with LLM analysis stub
2. **This Sprint**: Add GitHub Actions workflow template
3. **Next Sprint**: Implement LLM integration with caching
4. **Future**: MCP server for bidirectional AI communication
