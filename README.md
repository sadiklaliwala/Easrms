# EASRMS — Employee Asset & Service Request Management System

> **Version:** v1.0 | **Author:** Sadik Laliwala | **Duration:** 15 Working Days

A full-stack internal enterprise portal where employees raise asset and service requests, managers approve them, admins manage and assign them, and support users resolve them — with SLA tracking, escalation management, OAuth login, profile management, bulk imports, real-time notifications, and export capabilities built in.

**Live:** [easrms.sadiklaliwala.me](https://easrms.sadiklaliwala.me)
**Backend:** Render · **Frontend:** Vercel · **Database:** Neon (PostgreSQL) · **Custom Domain:** `easrms.sadiklaliwala.me`
**Repos:** [Azure DevOps](https://dev.azure.com/Rysun/Dotnet%20Team%20Interns%202026/_git/SadikProject) · [GitHub Mirror](https://github.com/sadiklaliwala/Easrms)

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Key Technical Highlights](#key-technical-highlights)
4. [Features Completed](#features-completed)
5. [Roles & Permissions](#roles--permissions)
6. [Status Flow](#status-flow)
7. [API Reference](#api-reference)
8. [Database Schema](#database-schema)
9. [Folder Structure](#folder-structure)
10. [Project Plan](#project-plan)
11. [API Response Format](#api-response-format)
12. [Error Codes](#error-codes)
13. [Setup & Installation](#setup--installation)
14. [Deployment](#deployment)
15. [Non-Functional Requirements](#non-functional-requirements)

---

## Project Overview

EASRMS is an internal enterprise portal built with **.NET 10 Web API** on the backend and **React 18 + TypeScript** on the frontend. It supports a complete request lifecycle — from creation by employees, through manager approval, admin assignment, support resolution, and final closure — with a full audit trail at every step.

The system went through a mid-project Change Request (CR-001) that added SLA tracking and escalation management on top of the core system, both of which are fully implemented. The project is deployed to production with a custom domain, CI/CD pipeline, real-time SignalR notifications, and a background email retry worker.

---

## Technology Stack

### Backend

| Purpose | Library / Tool |
|---|---|
| Framework | .NET 10 Web API |
| Architecture | Clean Architecture + CQRS with MediatR |
| ORM | EF Core (writes + simple reads) |
| Complex Queries | **Dapper** (dashboard, filtered listings, SLA queries) |
| Validation | **FluentValidation** |
| Authentication | JWT Bearer tokens (localStorage) + Refresh Token rotation |
| OAuth | Google, GitHub |
| Password Hashing | **BCrypt.Net** |
| Object Mapping | **AutoMapper** |
| API Documentation | Swagger (Swashbuckle) |
| Logging | **Serilog** |
| Exception Handling | Global Exception Middleware |
| Real-time | **SignalR** |
| File Storage | **Cloudinary** (signed upload) |
| Email | **Resend** |
| Export | **ClosedXML** (Excel) + custom PDF |
| Background Jobs | .NET `BackgroundService` (email retry worker) |
| CORS | Built-in .NET CORS Middleware |
| Testing | xUnit + Moq + FluentAssertions |
| CI | GitHub Actions |
| IDE | Visual Studio 2022 |
| Database | PostgreSQL (Neon) |

### Frontend

| Purpose | Library / Tool |
|---|---|
| Framework | React 18 with TypeScript + Vite |
| Routing | React Router v6 |
| State Management | **Redux Toolkit** |
| API Calls & Caching | **RTK Query** (createApi + fetchBaseQuery) |
| Form Handling | React Hook Form |
| Validation | Joi |
| UI Components | **MUI v5 (Material UI)** |
| Table / Grid | MUI DataGrid |
| Charts | **Recharts** |
| Notifications | React Hot Toast |
| Date Handling | date-fns |
| CSV Parsing | PapaParse |
| Spreadsheet | SheetJS |
| IDE | VS Code |
| API Testing | Postman |

---

## Key Technical Highlights

### Auth: localStorage Bearer Tokens (not HttpOnly Cookies)
The original SRS specified HttpOnly cookies for JWT storage. During production deployment, Chrome blocks HttpOnly cookies set by the backend (Render) as third-party cookies when the frontend is on a different root domain (Vercel) — even with `SameSite=None; Secure`. The auth was migrated to **localStorage Bearer tokens** sent as `Authorization: Bearer <token>` headers, which work correctly across origins. `credentials: 'include'` was removed from RTK Query's `fetchBaseQuery` accordingly.

### Why Dapper alongside EF Core?
EF Core handles all writes, simple reads, and entity management. **Dapper** is used specifically for dashboard queries, filtered paginated listings, and SLA summary queries — places where raw SQL gives significant performance gains over LINQ-generated queries with multiple joins. This hybrid approach gives the best of both worlds: clean entity management with EF Core, and raw SQL performance with Dapper for complex read scenarios.

### CQRS Pattern
The backend follows CQRS (Command Query Responsibility Segregation) via MediatR. Every feature is a Command (write) or Query (read) handler, keeping business logic isolated, testable, and easy to extend without touching unrelated code.

### Refresh Token with Rotation
Every login issues both an access token (short-lived) and a refresh token (longer-lived, stored in DB). On expiry, the refresh token generates a new access token. The revoke token endpoint invalidates the refresh token on logout, preventing reuse.

### RTK Query Cache Reset on Logout
`refetchOnFocus` and `refetchOnReconnect` are disabled globally to prevent re-authentication triggers after logout. On logout, both `clearCredentials()` and `api.util.resetApiState()` are dispatched to fully flush the RTK Query cache — without this, stale cached data from the previous session remains in memory.

### SignalR Real-time Notifications
SignalR is used for real-time push notifications on status changes and assignments. Because auth uses Bearer tokens (not cookies), the SignalR hub connection uses `accessTokenFactory` to inject the token from the Redux store on every connection, ensuring authenticated hub connections work correctly cross-origin.

### Background Email Retry Worker
A .NET `BackgroundService` runs continuously and retries failed email sends via Resend. Failed emails are queued and retried with backoff, ensuring transactional emails (OTP, status notifications) are eventually delivered even if the initial send fails.

### PostgreSQL Migration Gotchas
The project was migrated from SQL Server to PostgreSQL (Neon) for production. Key differences encountered:
- `COUNT` in PostgreSQL returns `long`, not `int` — requires explicit casting in Dapper result classes
- Dynamic Dapper queries produce casing-inconsistent columns on PostgreSQL — typed private result classes are used instead of `dynamic`
- `RETURNING id` replaces `SCOPE_IDENTITY()` for insert-and-return-id patterns
- All Dapper queries in `RequestQueries.cs` and `DashboardQueries.cs` were updated for PostgreSQL syntax

### FluentValidation
All business-level and DB-level validations are handled by FluentValidation validators, keeping controllers thin and validation logic centralised and reusable.

### SLA Status Computation
SLA status is computed both on the backend (stored `IsSLABreached` flag updated event-driven on every status change — not via a scheduled worker) and derived on the frontend using `getSLAStatus.ts`:

| Condition | Result |
|---|---|
| Request is Resolved or Closed | No breach regardless of date |
| `now > dueDate` | Breached |
| `now > dueDate − (SLAHours × 0.2)` | Nearing Breach |
| Otherwise | Within SLA |

The 20% nearing-breach threshold is defined as a named constant on both backend (`SLAConstants.cs`) and frontend (`sla.constants.ts`) — no magic numbers anywhere.

### Cloudinary Signed Upload
File attachments use Cloudinary signed uploads. The backend generates a signed upload signature that the frontend uses to upload directly to Cloudinary, keeping file data out of the API server entirely.

### Bulk Import
Users, Categories, and Requests all support bulk creation via file upload. Human-friendly lookup fields (e.g. `ManagerEmail`) resolve to FK IDs via pre-loaded dictionaries before insert — never stored directly.

### Server-Side Sorting
All listing APIs support `sortBy` + `sortAscending` parameters. Dapper uses whitelist-based dynamic `ORDER BY` clause construction to prevent SQL injection — only explicitly allowed column names are accepted.

### OAuth — Google & GitHub
Users can log in via Google or GitHub OAuth. The `UserAuthProviders` table stores linked providers per user. Google Cloud Console and GitHub OAuth app redirect URIs were updated to point to the production custom domain after deployment.

---

## Features Completed

### ✅ Core SRS Features

| Feature | Details |
|---|---|
| Authentication | Login, logout, JWT Bearer token, refresh token, revoke token, `/me` endpoint |
| Role-Based Access | 4 roles: Employee, Manager, Admin, Support User — enforced at API and UI level |
| Category Management | Create, edit, list (paginated + searchable + sortable), activate/deactivate, bulk import |
| Request Creation | Category, title, description, priority, attachment URL, auto-generated request number |
| Request Listing | Paginated, filterable (status, priority, category, date range), sortable |
| Request Detail | Full request data, comments, status history, role-based action buttons |
| Approval Flow | Manager approves or rejects; comment mandatory on rejection; status history updated |
| Assignment | Admin assigns approved/open requests to support users |
| Status Update | Support user moves request from Assigned → In Progress → Resolved |
| Close Request | Admin or Employee closes a Resolved request |
| Comments | Manager, Support, Admin can add comments on any request |
| Status History | Full audit trail for every status change with who changed it and when |
| Dashboard | Role-based metrics: counts by status, priority, category |
| Search & Pagination | All listing APIs support search, filter, pagination, and sorting |
| Lookup APIs | Support users dropdown, Managers dropdown for forms |

### ✅ CR-001: SLA Tracking & Escalation

| Feature | Details |
|---|---|
| SLA Hours on Category | Each category has a configurable SLA hours value |
| DueDate on Request | Calculated as `CreatedOn + SLAHours` when request is created |
| SLA Status | Within SLA / Nearing Breach / Breached — computed in real time |
| IsSLABreached Flag | Stored in DB, updated event-driven on every status transition |
| Escalation | Admin can escalate any eligible request with a mandatory reason |
| Escalation History | Full history table: `RequestEscalationHistory` with who escalated and when |
| SLA Dashboard | `/api/Dashboard/sla-summary` — WithinSLA, NearingBreach, Breached, Escalated counts |

### ✅ Beyond Scope (Extra Features Built)

| Feature | Details |
|---|---|
| Real-time Notifications | SignalR push notifications for status changes and assignments |
| Background Email Worker | .NET BackgroundService retries failed Resend email sends |
| Export to Excel | Export request list or single request as `.xlsx` with filters applied |
| Export to PDF | Export request list or single request as `.pdf` with filters applied |
| Cloudinary Upload | Signed upload support for file attachments on requests |
| OAuth Login | Google and GitHub social login |
| User Profile | View and update own profile (name, photo), OTP-based password change |
| OTP Password Change | Send OTP → verify OTP → get token → change password (secure flow) |
| Reopen Request | Closed or rejected requests can be reopened with a mandatory reason |
| Bulk Import — Users | Upload CSV to create multiple users at once |
| Bulk Import — Categories | Upload CSV to create multiple categories at once |
| Bulk Import — Requests | Upload CSV to create multiple requests at once |
| Unit Tests | xUnit + Moq + FluentAssertions covering core flows and CR regression |
| CI Pipeline | GitHub Actions runs tests and build on every push to main |
| Sorting | `sortBy` + `sortAscending` supported across Users, Categories, Requests |

---

## Roles & Permissions

| Role | Permissions |
|---|---|
| **Employee** | Create request, view own requests, view status and comments, close resolved requests, reopen requests |
| **Manager** | View team requests pending approval, approve or reject with comment, view dashboard |
| **Admin** | Manage users and categories, assign requests, escalate, update priority, full dashboard, bulk imports, export |
| **Support User** | View assigned requests, update status (In Progress, Resolved), add resolution comments |

---

## Status Flow

```
Open
 ├──► Pending Approval  (system, when category needs approval)
 │       ├──► Approved       (Manager)
 │       │       └──► Assigned   (Admin)
 │       └──► Rejected       (Manager, comment mandatory) ──► [Dead End]
 └──► Assigned          (Admin, when no approval needed)
         └──► In Progress    (Support User)
                 └──► Resolved     (Support User, resolution note mandatory)
                         └──► Closed        (Admin or Employee)
                                 └──► Open [Reopen]  (Employee or Admin, reason mandatory)
```

**Static Status Values:** Open · Pending Approval · Approved · Rejected · Assigned · In Progress · Resolved · Closed

---

## API Reference

### Auth — `/api/Auth`

| Method | Route | Purpose |
|---|---|---|
| POST | `/login` | Login with email/password, returns Bearer token |
| POST | `/logout` | End session |
| GET | `/me` | Get current logged-in user |
| POST | `/refresh-token` | Generate new access token using refresh token |
| POST | `/revoke-token` | Logout and invalidate refresh token |
| POST | `/oauth-login` | Login via Google or GitHub OAuth |
| POST | `/link-provider` | Link an OAuth provider to existing account |
| DELETE | `/unlink-provider` | Unlink an OAuth provider |
| GET | `/linked-providers` | Get all linked OAuth providers for current user |

### Users — `/api/User`

| Method | Route | Purpose |
|---|---|---|
| GET | `/` | List all users (paginated, searchable, sortable) — Admin only |
| GET | `/{id}` | Get single user detail |
| POST | `/` | Create new user — Admin only |
| PUT | `/{id}` | Edit user details |
| PUT | `/{id}/activate-deactivate` | Toggle user active status |
| DELETE | `/{id}` | Delete user |
| POST | `/bulk` | Bulk import users via CSV upload |

### Categories — `/api/Category`

| Method | Route | Purpose |
|---|---|---|
| GET | `/` | List all categories (paginated, searchable, sortable) |
| GET | `/{id}` | Get single category detail |
| POST | `/` | Create new category |
| PUT | `/{id}` | Edit category |
| PUT | `/{id}/activate-deactivate` | Toggle category active status |
| DELETE | `/{id}` | Delete category |
| POST | `/bulk` | Bulk import categories via CSV upload |

### Requests — `/api/Request`

| Method | Route | Purpose |
|---|---|---|
| POST | `/` | Create new request |
| GET | `/` | List requests (paginated, filtered, sorted) |
| GET | `/{id}` | Get request detail |
| POST | `/{id}/approval` | Manager approve or reject |
| POST | `/{id}/assign` | Admin assign to support user |
| POST | `/{id}/status` | Support/Admin update status |
| PUT | `/{id}/close` | Close a resolved request |
| POST | `/{id}/escalate` | Admin escalate request with reason |
| POST | `/{id}/reopen` | Reopen a closed/rejected request |
| POST | `/bulk` | Bulk import requests via CSV upload |

### Comments & History — `/api/requests/{requestId}`

| Method | Route | Purpose |
|---|---|---|
| POST | `/comments` | Add comment to request |
| GET | `/comments` | Get all comments for request |
| GET | `/history` | Get status history for request |

### Dashboard — `/api/Dashboard`

| Method | Route | Purpose |
|---|---|---|
| GET | `/summary` | Role-based request count metrics |
| GET | `/sla-summary` | SLA metrics (Within SLA, Nearing Breach, Breached, Escalated) |

### Lookup — `/api/Lookup`

| Method | Route | Purpose |
|---|---|---|
| GET | `/support-users` | Active support users for dropdown |
| GET | `/managers` | Managers list for dropdown |

### Profile — `/api/Profile`

| Method | Route | Purpose |
|---|---|---|
| GET | `/` | Get own profile |
| PUT | `/` | Update profile (name, photo URL) |
| POST | `/send-otp` | Send OTP for password change verification |
| POST | `/verify-otp` | Verify OTP, returns password change token |
| PUT | `/change-password` | Change password using OTP-verified token |

### Export — `/api/export`

| Method | Route | Purpose |
|---|---|---|
| GET | `/requests/excel` | Export filtered request list as Excel |
| GET | `/requests/pdf` | Export filtered request list as PDF |
| GET | `/requests/{id}/excel` | Export single request as Excel |
| GET | `/requests/{id}/pdf` | Export single request as PDF |

### Cloudinary — `/api/Cloudinary`

| Method | Route | Purpose |
|---|---|---|
| POST | `/sign` | Generate signed upload URL for Cloudinary |

---

## Database Schema

### Users
| Column | Type | Notes |
|---|---|---|
| UserId | Guid | PK |
| FullName | string | Required |
| Email | string | Required, Unique |
| PasswordHash | string | BCrypt hashed |
| RoleId | Guid | FK → Roles |
| ManagerId | Guid? | FK → Users (self-reference) |
| IsActive | bool | Default true |
| CreatedOn | DateTime | |
| UpdatedOn | DateTime? | |
| LastLoginOn | DateTime? | |
| RefreshToken | string? | |
| RefreshTokenExpiryOn | DateTime? | |

### Roles
| Column | Type | Notes |
|---|---|---|
| RoleId | Guid | PK |
| RoleName | string | Required, Unique |

### RequestCategory
| Column | Type | Notes |
|---|---|---|
| CategoryId | Guid | PK |
| CategoryName | string | Required, Unique |
| IsApprovalRequired | bool | Default false |
| SLAHours | int | Required, > 0 (default 24 for existing seeded rows) |
| IsActive | bool | Default true |
| CreatedOn | DateTime | |
| UpdatedOn | DateTime? | |

### ServiceRequest
| Column | Type | Notes |
|---|---|---|
| RequestId | Guid | PK |
| RequestNumber | string | Required, Unique, auto-generated |
| EmployeeId | Guid | FK → Users |
| CategoryId | Guid | FK → RequestCategory |
| Title | string | Required |
| Description | string | Required |
| Priority | string | Low / Medium / High |
| Status | string | One of 8 status values |
| AssignedTo | Guid? | FK → Users |
| DueDate | DateTime? | CreatedOn + SLAHours (null for pre-CR requests) |
| IsSLABreached | bool | Default false, set event-driven on status updates |
| IsEscalated | bool | Default false |
| EscalatedOn | DateTime? | |
| EscalatedBy | Guid? | FK → Users |
| EscalationReason | string? | Max 500 chars |
| CreatedOn | DateTime | |
| UpdatedOn | DateTime? | |
| ResolvedOn | DateTime? | |
| ClosedOn | DateTime? | |
| ClosedBy | Guid? | FK → Users |
| RejectionReason | string? | |

### RequestComment
| Column | Type | Notes |
|---|---|---|
| CommentId | Guid | PK |
| RequestId | Guid | FK → ServiceRequest |
| CommentBy | Guid | FK → Users |
| CommentText | string | Required |
| CommentType | string | Required |
| CreatedOn | DateTime | |
| IsDeleted | bool | Soft delete, default false |

### RequestStatusHistory
| Column | Type | Notes |
|---|---|---|
| HistoryId | Guid | PK |
| RequestId | Guid | FK → ServiceRequest |
| OldStatus | string? | Null on initial creation |
| NewStatus | string | Required |
| ChangedBy | Guid | FK → Users |
| ChangedOn | DateTime | |
| Remarks | string? | |

### RequestEscalationHistory *(CR-001)*
| Column | Type | Notes |
|---|---|---|
| EscalationId | Guid | PK |
| RequestId | Guid | FK → ServiceRequest |
| EscalatedBy | Guid | FK → Users |
| EscalatedOn | DateTime | |
| EscalationReason | string | Max 500 chars |
| CreatedOn | DateTime | |

---

## Folder Structure

### Backend (`Easrms-BackEnd/`)

```
Easrms.API/              → Controllers, Middleware, Extensions, Program.cs
Easrms.Application/      → DTOs, CQRS Features (Commands + Queries + Handlers), AutoMapper Profiles
Easrms.Domain/           → Entities (pure domain models, no dependencies)
Easrms.Infrastructure/   → EF Core DbContext, Dapper Context, Repositories, JwtService, DapperQueries
Easrms.Common/           → ApiResponse wrapper, Constants (Role, Status, Priority, SLA), Helpers
```

### Frontend (`Easrms-Frontend/src/`)

```
components/common/       → Reusable UI: buttons, forms, layout, modals, tables, filters, dashboard charts, SLA badge
components/request/      → Request-specific: action buttons, comment box, history timeline, SLA info, escalation banner
pages/                   → Auth, Dashboard, Categories, Requests, Users, Approval, Assignment, Support, Profile
store/api/               → RTK Query endpoints split by feature (auth, category, request, comment, dashboard, lookup)
store/slices/            → Redux slices (authSlice)
types/                   → TypeScript interfaces per feature (auth, category, request, comment, dashboard, common)
constants/               → priority, role, status, sla constants
utils/                   → buildQueryParams, canPerformAction, formatDate, getSLAStatus, getPriorityColor, etc.
routes/                  → AppRoutes, ProtectedRoute, RoleBasedRoute
theme/                   → MUI theme overrides
```

---

## Project Plan

| Day | Activity | Output |
|---|---|---|
| Day 1 | Requirement walkthrough | Clarification log |
| Day 2 | Task breakdown and estimation | Task sheet with estimates |
| Day 3 | Technical design | DB design, API contracts, React plan |
| Day 4 | Base setup | Git repo, project structure, DB migration, React shell |
| Day 5 | Core backend | Auth, Category, Request APIs |
| Day 6 | Core frontend | Login, role routing, category screen, create request screen |
| Day 7 | First review | Demo, review comments, revised plan |
| Day 8 | CR received | Read CR-001, no coding |
| Day 9 | CR impact analysis | Impact document, revised estimates |
| Day 10 | DB migration + approval/assignment flows | SLA tables, manager and admin screens and APIs |
| Day 11 | CR implementation | SLA + escalation in separate branch, CR test cases |
| Day 12 | Support, dashboard, integration | All screens connected end to end |
| Day 13 | Testing and bug fixes | Test evidence, regression results |
| Day 14 | Cleanup and demo prep | Clean code, demo script, known issues list |
| Day 15 | Final demo | Working demo, code walkthrough, Q&A |

---

## API Response Format

All APIs return a consistent response wrapper:

**Success**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Request created successfully",
  "data": {},
  "errors": null
}
```

**Error**
```json
{
  "success": false,
  "statusCode": 400,
  "message": "Validation failed",
  "data": null,
  "errors": ["Title is required", "Category is required"]
}
```

**Paginated List**
```json
{
  "success": true,
  "statusCode": 200,
  "message": "OK",
  "data": {
    "items": [],
    "pagination": {
      "pageNumber": 1,
      "pageSize": 10,
      "totalCount": 42,
      "totalPages": 5
    }
  },
  "errors": null
}
```

---

## Error Codes

| Code | Meaning | When Used |
|---|---|---|
| 200 | OK | Successful GET and PUT |
| 201 | Created | Successful POST, new record created |
| 400 | Bad Request | Validation failed, missing required fields |
| 401 | Unauthorized | Not logged in or token expired |
| 403 | Forbidden | Logged in but insufficient permissions |
| 404 | Not Found | Request, user, or category ID does not exist |
| 409 | Conflict | Duplicate name or invalid status transition |
| 500 | Internal Server Error | Unexpected server-side error |

---

## Setup & Installation

### Prerequisites
- .NET 10 SDK
- PostgreSQL (local) or a Neon connection string
- Node.js 20+
- Visual Studio 2022 / VS Code

### Backend

```bash
# Clone the repo
git clone <repo-url>
cd Easrms-BackEnd

# Set connection string and secrets in appsettings.Development.json
# Never commit appsettings.json with real secrets — use environment variables on host

# Run migrations
cd Easrms.API
dotnet ef database update

# Run the API
dotnet run
# Swagger available at: https://localhost:{port}/swagger
```

### Frontend

```bash
cd Easrms-Frontend

# Install dependencies
npm install

# Create .env
VITE_API_BASE_URL=https://localhost:{port}

# Start dev server
npm run dev
```

### Environment Variables (Backend)

| Key | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__Key` | JWT signing secret (min 32 chars) |
| `Jwt__Issuer` | JWT issuer |
| `Jwt__Audience` | JWT audience |
| `Jwt__AccessTokenExpiryMinutes` | Access token lifetime |
| `Jwt__RefreshTokenExpiryDays` | Refresh token lifetime |
| `Google__ClientId` | Google OAuth client ID |
| `Google__ClientSecret` | Google OAuth client secret |
| `GitHub__ClientId` | GitHub OAuth client ID |
| `GitHub__ClientSecret` | GitHub OAuth client secret |
| `Cloudinary__CloudName` | Cloudinary cloud name |
| `Cloudinary__ApiKey` | Cloudinary API key |
| `Cloudinary__ApiSecret` | Cloudinary API secret |
| `Resend__ApiKey` | Resend email API key |
| `Resend__FromEmail` | Sender email address |

> **Never commit `appsettings.json` with real values.** Add it to `.gitignore` and use `git rm --cached` before first push if already tracked.

### Run Tests

```bash
cd Easrms-BackEnd
dotnet test
```

---

## Deployment

| Layer | Platform | Notes |
|---|---|---|
| Backend API | Render | Environment variables set in Render dashboard |
| Frontend | Vercel | `VITE_API_BASE_URL` set in Vercel project settings |
| Database | Neon (PostgreSQL) | Connection string in Render env vars |
| Custom Domain | `easrms.sadiklaliwala.me` | DNS pointed to Vercel; CORS updated to allow this origin |
| File Storage | Cloudinary | Signed uploads; no files pass through API server |
| Email | Resend | Transactional email (OTP, notifications) |

**Cross-origin note:** Backend (Render) and frontend (Vercel) are on different root domains. Chrome blocks HttpOnly cookies in this topology as third-party cookies. Auth uses localStorage Bearer tokens to avoid this. Google OAuth and GitHub OAuth redirect URIs were updated in their respective consoles to point to `easrms.sadiklaliwala.me` after go-live. SignalR `accessTokenFactory` reads the token from the Redux store for authenticated hub connections.

---

## Non-Functional Requirements

| Requirement | Implementation |
|---|---|
| Security | Role-based API authorization, JWT Bearer tokens, BCrypt password hashing, OTP-verified password changes, secrets in environment variables only |
| Validation | Joi on frontend, FluentValidation on backend — both layers always validated |
| Error Handling | Global Exception Middleware, consistent `ApiResponse<T>` wrapper, no raw exceptions exposed |
| Performance | Dapper for complex queries, server-side pagination + sorting on all listing APIs, Cloudinary for file offloading |
| Maintainability | CQRS pattern, Clean Architecture, AutoMapper, reusable MUI components |
| Logging | Serilog structured logging for errors and business-critical actions |
| Audit Trail | `RequestStatusHistory` and `RequestEscalationHistory` tables capture every meaningful action |
| Real-time | SignalR for push notifications on status changes and assignments |
| Reliability | Background email retry worker ensures transactional emails are eventually delivered |
| Testing | xUnit + Moq + FluentAssertions unit tests; CI via GitHub Actions |

---

*EASRMS v1.0 | Sadik Laliwala | Rysun Internship 2026*
