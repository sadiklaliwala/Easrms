# EASRMS — Employee Asset & Service Request Management System

> **Version:** v1.0 | **Author:** Sadik Laliwala | **Duration:** 15 Working Days

A full-stack internal enterprise portal where employees raise asset and service requests, managers approve them, admins manage and assign them, and support users resolve them — with SLA tracking, escalation management, OAuth login, profile management, bulk imports, and export capabilities built in.

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

---

## Project Overview

EASRMS is an internal enterprise portal built with **.NET 8 Web API** on the backend and **React + TypeScript** on the frontend. It supports a complete request lifecycle — from creation by employees, through manager approval, admin assignment, support resolution, and final closure — with a full audit trail at every step.

The system went through a mid-project Change Request (CR-001) that added SLA tracking and escalation management on top of the core system, both of which are fully implemented.

---

## Technology Stack

### Backend

| Purpose | Library / Tool |
|---|---|
| Framework | .NET 8 Web API |
| Architecture | CQRS with MediatR pattern |
| ORM | EF Core 8 |
| Complex Queries | **Dapper** (dashboard, filtered listings, SLA queries) |
| Validation | **FluentValidation** |
| Authentication | JWT in HttpOnly Cookies + OAuth (Google, Microsoft, GitHub etc.) |
| Refresh Token | Secure refresh token with rotation and revocation |
| Password Hashing | **BCrypt.Net** |
| Object Mapping | **AutoMapper** |
| API Documentation | Swagger (Swashbuckle) |
| Logging | **Serilog** |
| Exception Handling | Global Exception Middleware |
| File Storage | Cloudinary (signed upload) |
| Export | Excel (EPPlus / ClosedXML) + PDF export |
| CORS | Built-in .NET CORS Middleware |
| IDE | Visual Studio 2022 |
| DB | SQL Server + SSMS |

### Frontend

| Purpose | Library / Tool |
|---|---|
| Framework | React 18 with TypeScript |
| Routing | React Router v6 |
| State Management | **Redux Toolkit** |
| API Calls & Caching | **RTK Query** (createApi + fetchBaseQuery) |
| Form Handling | React Hook Form |
| Validation | Joi |
| UI Components | **MUI (Material UI)** |
| Table / Grid | MUI DataGrid |
| Charts | **Recharts** |
| Notifications | React Hot Toast |
| Date Handling | date-fns |
| IDE | VS Code |
| API Testing | Postman |

---

## Key Technical Highlights

### Why Dapper alongside EF Core?
EF Core handles all writes, simple reads, and entity management. **Dapper** is used specifically for dashboard queries, filtered paginated listings, and SLA summary queries — places where raw SQL gives significant performance gains over LINQ-generated queries with multiple joins. This hybrid approach gives the best of both worlds: clean entity management with EF Core, and raw SQL performance with Dapper for complex read scenarios.

### CQRS Pattern
The backend follows CQRS (Command Query Responsibility Segregation) via MediatR. Every feature is a Command (write) or Query (read) handler, keeping business logic isolated, testable, and easy to extend without touching unrelated code.

### JWT in HttpOnly Cookies
JWTs are stored in HttpOnly cookies (not localStorage), making them inaccessible to JavaScript and protecting against XSS attacks. The frontend uses `credentials: 'include'` in RTK Query's `fetchBaseQuery` to send cookies automatically with every request.

### Refresh Token with Rotation
Every login issues both an access token (short-lived) and a refresh token (longer-lived, stored in DB). On expiry, the refresh token generates a new access token. Revoke token endpoint invalidates the refresh token on logout, preventing reuse.

### FluentValidation
All business-level and DB-level validations are handled by FluentValidation validators, keeping controllers thin and validation logic centralised and reusable.

### BCrypt Password Hashing
Passwords are never stored in plain text. BCrypt with a work factor is used for hashing, making brute-force attacks computationally expensive.

### AutoMapper
All entity-to-DTO and DTO-to-entity mappings are handled by AutoMapper profiles, keeping handlers free of manual mapping code.

### Serilog Structured Logging
Serilog provides structured, queryable logs for API errors, important business actions (approvals, assignments, escalations), and exception details — making production debugging significantly easier than plain text logs.

### SLA Status Computation
SLA status is computed both on the backend (stored `IsSLABreached` flag updated on every status change) and derived on the frontend using `getSLAStatus.ts`:

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
Users, Categories, and Requests all support bulk creation via file upload (`multipart/form-data`), useful for seeding or migrating large datasets without manual entry.

### OAuth / Social Login
Users can log in via OAuth providers (Google, Microsoft, GitHub, etc.) and link or unlink multiple providers to the same account. The `AuthProviderEnum` supports 5 providers.

---

## Features Completed

### ✅ Core SRS Features

| Feature | Details |
|---|---|
| Authentication | Login, logout, JWT in HttpOnly cookie, refresh token, revoke token, `/me` endpoint |
| Role-Based Access | 4 roles: Employee, Manager, Admin, Support User — enforced at API and UI level |
| Category Management | Create, edit, list (paginated + searchable + sortable), activate/deactivate, delete, bulk import |
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
| IsSLABreached Flag | Stored in DB, updated on every status transition |
| Escalation | Admin can escalate any eligible request with a mandatory reason |
| Escalation History | Full history table: `RequestEscalationHistory` with who escalated and when |
| SLA Dashboard | `/api/Dashboard/sla-summary` — WithinSLA, NearingBreach, Breached, Escalated counts |

### ✅ Beyond Scope (Extra Features Built)

| Feature | Details |
|---|---|
| Export to Excel | Export request list or single request as `.xlsx` with filters applied |
| Export to PDF | Export request list or single request as `.pdf` with filters applied |
| Cloudinary Upload | Signed upload support for file attachments on requests |
| OAuth Login | Social login via OAuth providers (Google, Microsoft, GitHub, etc.) |
| Provider Link/Unlink | Users can link or unlink multiple OAuth providers to one account |
| User Profile | View and update own profile (name, photo), OTP-based password change |
| OTP Password Change | Send OTP → verify OTP → get token → change password (secure flow) |
| Reopen Request | Closed or rejected requests can be reopened with a mandatory reason |
| Bulk Import — Users | Upload file to create multiple users at once |
| Bulk Import — Categories | Upload file to create multiple categories at once |
| Bulk Import — Requests | Upload file to create multiple requests at once |
| Delete | Soft delete on User and Category |
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
| POST | `/login` | Login with email/password, sets JWT cookie |
| POST | `/logout` | Clear cookie and end session |
| GET | `/me` | Get current logged-in user |
| POST | `/refresh-token` | Generate new access token using refresh token |
| POST | `/revoke-token` | Logout and invalidate refresh token |
| POST | `/oauth-login` | Login via OAuth provider (Google, Microsoft, etc.) |
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
| DELETE | `/{id}` | Hard delete user |
| POST | `/bulk` | Bulk import users via file upload |

### Categories — `/api/Category`

| Method | Route | Purpose |
|---|---|---|
| GET | `/` | List all categories (paginated, searchable, sortable) |
| GET | `/{id}` | Get single category detail |
| POST | `/` | Create new category |
| PUT | `/{id}` | Edit category |
| PUT | `/{id}/activate-deactivate` | Toggle category active status |
| DELETE | `/{id}` | Hard delete category |
| POST | `/bulk` | Bulk import categories via file upload |

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
| POST | `/bulk` | Bulk import requests via file upload |

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
| SLAHours | int | Required, > 0 (default 24 for seeded data) |
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
| DueDate | DateTime? | CreatedOn + SLAHours |
| IsSLABreached | bool | Default false |
| IsEscalated | bool | Default false |
| EscalatedOn | DateTime? | |
| EscalatedBy | Guid? | FK → Users |
| EscalationReason | string? | |
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
| OldStatus | string? | Nullable (null on creation) |
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
Easrms.Application/      → DTOs, CQRS Features (Commands + Queries), AutoMapper Profiles
Easrms.Domain/           → Entities (pure domain models, no dependencies)
Easrms.Infrastructure/   → EF Core DbContext, Dapper Context, Repositories, JwtService, DapperQueries
Easrms.Common/           → ApiResponse wrapper, Constants (Role, Status, Priority, SLA), Helpers
```

### Frontend (`Easrms-Frontend/src/`)

```
components/common/       → Reusable UI: buttons, forms, layout, modals, tables, filters, dashboard charts
components/request/      → Request-specific: action buttons, comment box, history timeline, SLA info, escalation banner
pages/                   → Auth, Dashboard, Categories, Requests, Users, Approval, Assignment, Support, Profile
store/api/               → RTK Query endpoints split by feature (auth, category, request, comment, dashboard, lookup)
store/slices/            → Redux slices (authSlice)
types/                   → TypeScript types per feature
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
- .NET 8 SDK
- SQL Server (local instance)
- Node.js 18+
- Visual Studio 2022 / VS Code

### Backend

```bash
# Clone the repo
git clone <repo-url>
cd Easrms-BackEnd

# Update connection string in appsettings.Development.json
# "ConnectionStrings": { "DefaultConnection": "Server=.;Database=EasrmsDb;Trusted_Connection=True;" }

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

# Set API base URL in .env
VITE_API_BASE_URL=https://localhost:{port}

# Start dev server
npm run dev
```

> **Important:** `credentials: 'include'` is set in RTK Query's `fetchBaseQuery` so JWT cookies are sent with every request. Do not change this.

---

## Non-Functional Requirements

| Requirement | Implementation |
|---|---|
| Security | Role-based API authorization, JWT in HttpOnly cookies, BCrypt password hashing, OTP-verified password changes |
| Validation | Joi on frontend, FluentValidation on backend — both layers always validated |
| Error Handling | Global Exception Middleware, consistent `ApiResponse<T>` wrapper, no raw exceptions exposed |
| Performance | Dapper for complex queries, pagination on all listing APIs, Cloudinary for file offloading |
| Maintainability | CQRS pattern, layered architecture, AutoMapper, reusable MUI components |
| Logging | Serilog structured logging for errors and business-critical actions |
| Audit Trail | `RequestStatusHistory` and `RequestEscalationHistory` tables capture every meaningful action |

---

*EASRMS v1.0 | Sadik Laliwala | 11-05-2026*
