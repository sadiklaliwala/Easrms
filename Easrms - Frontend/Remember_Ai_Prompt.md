You are working on a React TypeScript frontend for a project called
Easrms (Employee Asset and Service Request Management System).

TECH STACK:

- React with TypeScript
- Vite
- Redux Toolkit
- RTK Query with createApi and fetchBaseQuery
- credentials: 'include' is set in fetchBaseQuery (JWT in HttpOnly cookies)
- React Router v6
- MUI (Material UI) v9.x.x
- MUI DataGrid
- React Hook Form
- Joi for validation
- React Hot Toast
- Recharts
- date-fns

WHAT IS ALREADY DONE:

- React TypeScript project created with Vite
- All npm packages installed
- Folder structure set up
- RTK Query configured with credentials include
- Redux store set up

ROLES (hardcoded, no API needed):

- Admin
- Manager
- Employee
- Support User

STATUS VALUES (hardcoded, no API needed):

- Open
- Pending Approval
- Approved
- Rejected
- Assigned
- In Progress
- Resolved
- Closed

PRIORITY VALUES (hardcoded, no API needed):

- High
- Medium
- Low

BACKEND API BASE URL:

- https://localhost:7252/api

ALL 28 API ROUTES:

1.  POST /api/auth/login
2.  POST /api/auth/logout
3.  GET /api/auth/me
4.  GET /api/users
5.  GET /api/users/{id}
6.  POST /api/users
7.  PUT /api/users/{id}
8.  PUT /api/users/{id}/activate-deactivate
9.  GET /api/categories
10. GET /api/categories/{id}
11. POST /api/categories
12. PUT /api/categories/{id}
13. PUT /api/categories/{id}/activate-deactivate
14. POST /api/requests
15. GET /api/requests
16. GET /api/requests/{id}
17. POST /api/requests/{id}/approval
18. POST /api/requests/{id}/assign
19. POST /api/requests/{id}/status
20. PUT /api/requests/{id}/close
21. POST /api/requests/{id}/comments
22. GET /api/requests/{id}/comments
23. GET /api/requests/{id}/history
24. GET /api/dashboard/summary
25. GET /api/lookup/support-users
26. GET /api/lookup/managers
27. POST /api/auth/refresh-token
28. POST /api/auth/revoke-token

API RESPONSE FORMAT (all APIs return this wrapper):
{
"success": true/false,
"statusCode": 200/400/401/403/404/409/500,
"message": "string",
"data": {},
"errors": []
}

AUTHENTICATION:

- JWT stored in HttpOnly cookies
- Access token expires in 15 minutes
- Refresh token expires in 7 days
- Auto refresh using baseQueryWithReauth wrapper in RTK Query
- credentials: 'include' mandatory in fetchBaseQuery

IMPORTANT RULES:

- Never use localStorage for JWT
- credentials include is always set
- All API calls go through RTK Query
- Role based routing and navigation
- MUI components only for UI
- React Hook Form with Joi for all forms
- React Hot Toast for all notifications

WHERE WE ARE NOW:

- Redux store is set up
- RTK Query is configured
- Now continuing with React Router v6 base routes setup
