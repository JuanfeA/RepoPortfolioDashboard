# Tech Stack Decision Checklist

Use this checklist to help decide on your tech stack. Check the options that apply to your situation.

---

## Team Skills & Experience

### Backend
- [ ] Strong .NET/C# experience
- [ ] Strong Node.js/TypeScript experience  
- [ ] Strong Python experience
- [ ] Familiar with serverless (Azure Functions/AWS Lambda)

### Frontend
- [ ] Strong React experience
- [ ] Strong Blazor/Razor experience
- [ ] Prefer component libraries over custom CSS
- [ ] Need charting/visualization libraries

### DevOps
- [ ] Already using Azure  
- [ ] Already using AWS
- [ ] Prefer managed services over self-hosted
- [ ] Need CI/CD pipelines immediately

---

## Project Constraints

### Budget
- [ ] $0-25/month (hobby/startup)
- [ ] $25-100/month (small team)
- [ ] $100+/month (enterprise)

### Timeline
- [ ] MVP needed in < 2 weeks
- [ ] MVP needed in 2-4 weeks
- [ ] MVP needed in 1-2 months
- [ ] No strict deadline

### Users
- [ ] Just me / small team (< 10)
- [ ] Organization (10-100)
- [ ] Enterprise (100+)

---

## Feature Priorities

### Must Have (Phase 1)
- [ ] Basic repo list with metadata
- [ ] Simple scoring display
- [ ] GitHub sync (manual ok)

### Should Have (Phase 2)  
- [ ] Configurable scoring weights
- [ ] Historical score tracking
- [ ] Automated nightly sync

### Nice to Have (Phase 3+)
- [ ] Real-time updates
- [ ] Notifications (Slack/Teams)
- [ ] MCP/AI agent integration
- [ ] Multi-org support

---

## Technical Requirements

### Data
- [ ] Need to store sensitive data (encrypted)
- [ ] Need versioning/audit trail
- [ ] Expect 100+ repositories
- [ ] Need API for external integrations

### Auth
- [ ] Single user (no auth needed)
- [ ] Team with simple password auth
- [ ] Need SSO (Azure AD, GitHub OAuth)
- [ ] Need role-based permissions

### Hosting
- [ ] Must be on Azure
- [ ] Open to any cloud provider
- [ ] Prefer serverless/managed
- [ ] Need on-premises option

---

## Quick Recommendation

Based on common patterns:

| If you checked... | Recommended Stack |
|-------------------|-------------------|
| .NET experience + Azure | Blazor + Azure SQL + Azure Functions |
| React experience + fast MVP | Next.js + Supabase + Vercel Cron |
| Lowest budget + simple needs | Supabase (free) + Vercel (hobby) |
| Enterprise + many users | .NET API + React + Azure SQL |
| MCP/AI focus | PostgreSQL + Node.js or Python API |

---

## Your Decision

**Database**: ____________________

**Backend API**: ____________________

**Frontend**: ____________________

**Sync Jobs**: ____________________

**Auth**: ____________________

**Hosting**: ____________________

---

## Notes

_Add your notes and considerations here..._
