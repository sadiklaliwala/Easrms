import { lazy, Suspense } from "react";
import { Routes, Route, Navigate } from "react-router-dom";

import ProtectedRoute from "./ProtectedRoute";
import RoleBasedRoute from "./RoleBasedRoute";
import AppLayout from "../components/common/layout/AppLayout";
import AppLoader from "../components/common/feedback/AppLoader";

import { useAppSelector } from "../hooks/useAppSelector";
import { ROLES } from "../constants/role.constants";

// ─── Lazy Loaded Pages ────────────────────────────────────────────────────────
const LoginPage = lazy(() => import("../pages/auth/LoginPage"));
const DashboardPage = lazy(() => import("../pages/dashboard/DashboardPage"));
const UserPage = lazy(() => import("../pages/users/UserPage"));
const CategoryPage = lazy(() => import("../pages/categories/CategoryPage"));
const CreateRequestPage = lazy(
  () => import("../pages/requests/CreateRequestPage"),
);
const RequestListPage = lazy(() => import("../pages/requests/RequestListPage"));
const RequestDetailPage = lazy(
  () => import("../pages/requests/RequestDetailPage"),
);
const ApprovalQueuePage = lazy(
  () => import("../pages/approval/ApprovalQueuePage"),
);
const AssignmentPage = lazy(() => import("../pages/assignment/AssignmentPage"));
const SupportTaskPage = lazy(() => import("../pages/support/SupportTaskPage"));
const OAuthCallbackPage = lazy(
  () => import("../pages/auth/OAuthCallbackPage"),
);
const ProfilePage = lazy(() => import("../pages/profile/ProfilePage"));

const AppRoutes = () => {
  return (
    <Suspense fallback={<AppLoader />}>
      <Routes>
        {/* Public */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/auth/callback" element={<OAuthCallbackPage />} />
        <Route path="/unauthorized" element={<UnauthorizedPage />} />

        {/* Protected — all routes inside require authentication */}
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            {/* Dashboard — Admin, Manager and Employee */}
            <Route
              element={
                <RoleBasedRoute
                  allowedRoles={[ROLES.ADMIN, ROLES.MANAGER, ROLES.EMPLOYEE]}
                />
              }
            >
              <Route path="/dashboard" element={<DashboardPage />} />
            </Route>

            {/* Users — Admin only */}
            <Route element={<RoleBasedRoute allowedRoles={[ROLES.ADMIN]} />}>
              <Route path="/users" element={<UserPage />} />
            </Route>

            {/* Categories — Admin only */}
            <Route element={<RoleBasedRoute allowedRoles={[ROLES.ADMIN]} />}>
              <Route path="/categories" element={<CategoryPage />} />
            </Route>

            {/* Requests — All roles */}
            <Route
              element={
                <RoleBasedRoute
                  allowedRoles={[
                    ROLES.ADMIN,
                    ROLES.MANAGER,
                    ROLES.EMPLOYEE,
                    ROLES.SUPPORT_USER,
                  ]}
                />
              }
            >
              <Route path="/requests" element={<RequestListPage />} />
              <Route path="/requests/:id" element={<RequestDetailPage />} />
            </Route>

            {/* Create Request — Employee only */}
            <Route element={<RoleBasedRoute allowedRoles={[ROLES.EMPLOYEE]} />}>
              <Route path="/requests/create" element={<CreateRequestPage />} />
            </Route>

            {/* Approval Queue — Manager only */}
            <Route element={<RoleBasedRoute allowedRoles={[ROLES.MANAGER]} />}>
              <Route path="/approvals" element={<ApprovalQueuePage />} />
            </Route>

            {/* Assignment — Admin only */}
            <Route element={<RoleBasedRoute allowedRoles={[ROLES.ADMIN]} />}>
              <Route path="/assignments" element={<AssignmentPage />} />
            </Route>

            {/* Support Tasks — Support User only */}
            <Route
              element={<RoleBasedRoute allowedRoles={[ROLES.SUPPORT_USER]} />}
            >
              <Route path="/my-tasks" element={<SupportTaskPage />} />
            </Route>

            {/* Profile — All roles */}
            <Route
              element={
                <RoleBasedRoute
                  allowedRoles={[
                    ROLES.ADMIN,
                    ROLES.MANAGER,
                    ROLES.EMPLOYEE,
                    ROLES.SUPPORT_USER,
                  ]}
                />
              }
            >
              <Route path="/profile" element={<ProfilePage />} />
            </Route>

            {/* Default redirect */}
            <Route path="/" element={<DefaultRedirect />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Route>
      </Routes>
    </Suspense>
  );
};

// Redirects to the correct landing page based on role
const DefaultRedirect = () => {
  const { roleName } = useAppSelector((state) => state.auth);

  if (roleName === ROLES.ADMIN || roleName === ROLES.MANAGER) {
    return <Navigate to="/dashboard" replace />;
  }
  if (roleName === ROLES.EMPLOYEE) {
    return <Navigate to="/requests" replace />;
  }
  if (roleName === ROLES.SUPPORT_USER) {
    return <Navigate to="/my-tasks" replace />;
  }

  return <Navigate to="/login" replace />;
};

// Inline unauthorized page — simple, no separate file needed
const UnauthorizedPage = () => {
  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        height: "100vh",
      }}
    >
      <h2>403 — Access Denied</h2>
      <p>You do not have permission to view this page.</p>
    </div>
  );
};

export default AppRoutes;
