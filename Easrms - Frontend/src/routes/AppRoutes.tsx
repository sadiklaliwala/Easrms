// import { Routes, Route, Navigate } from 'react-router-dom';
// import ProtectedRoute from './ProtectedRoute';
// import RoleBasedRoute from './RoleBasedRoute';
// import LoginPage from '../pages/auth/LoginPage';
// import DashboardPage from '../pages/dashboard/DashboardPage';
// import RequestListPage from '../pages/requests/RequestListPage';
// import RequestDetailPage from '../pages/requests/RequestDetailPage';
// import CreateRequestPage from '../pages/requests/CreateRequestPage';
// import CategoryPage from '../pages/categories/CategoryPage';
// import UserPage from '../pages/users/UserPage';
// import ApprovalQueuePage from '../pages/approval/ApprovalQueuePage';
// import AssignmentPage from '../pages/assignment/AssignmentPage';
// import SupportTaskPage from '../pages/support/SupportTaskPage';
// import NotFound from '../pages/NotFound';
// import { RoleConstants } from '../constants/role.constants';

// const AppRoutes = () => {
//   return (
//     <Routes>

//       {/* Public */}
//       <Route path="/login" element={<LoginPage />} />

//       {/* Protected Routes */}
//       <Route element={<ProtectedRoute />}>

//         {/* All Roles */}
//         <Route path="/dashboard" element={<DashboardPage />} />
//         <Route path="/requests" element={<RequestListPage />} />
//         <Route path="/requests/:id" element={<RequestDetailPage />} />

//         {/* Employee Only */}
//         <Route element={<RoleBasedRoute allowedRoles={[RoleConstants.Employee]} />}>
//           <Route path="/requests/create" element={<CreateRequestPage />} />
//         </Route>

//         {/* Admin Only */}
//         <Route element={<RoleBasedRoute allowedRoles={[RoleConstants.Admin]} />}>
//           <Route path="/categories" element={<CategoryPage />} />
//           <Route path="/users" element={<UserPage />} />
//           <Route path="/assignment" element={<AssignmentPage />} />
//         </Route>

//         {/* Manager Only */}
//         <Route element={<RoleBasedRoute allowedRoles={[RoleConstants.Manager]} />}>
//           <Route path="/approval" element={<ApprovalQueuePage />} />
//         </Route>

//         {/* Support User Only */}
//         <Route element={<RoleBasedRoute allowedRoles={[RoleConstants.SupportUser]} />}>
//           <Route path="/support/tasks" element={<SupportTaskPage />} />
//         </Route>

//       </Route>

//       {/* Default */}
//       <Route path="/" element={<Navigate to="/login" />} />
//       <Route path="*" element={<NotFound />} />

//     </Routes>
//   );
// };

// export default AppRoutes;

import { Routes, Route, Navigate } from 'react-router-dom';

import ProtectedRoute from './ProtectedRoute';
import RoleBasedRoute from './RoleBasedRoute';
import AppLayout from '../components/common/layout/AppLayout';

import LoginPage from '../pages/auth/LoginPage';
import DashboardPage from '../pages/dashboard/DashboardPage';
import UserPage from '../pages/users/UserPage';
import CategoryPage from '../pages/categories/CategoryPage';
import CreateRequestPage from '../pages/requests/CreateRequestPage';
import RequestListPage from '../pages/requests/RequestListPage';
import RequestDetailPage from '../pages/requests/RequestDetailPage';
import ApprovalQueuePage from '../pages/approval/ApprovalQueuePage';
import AssignmentPage from '../pages/assignment/AssignmentPage';
import SupportTaskPage from '../pages/support/SupportTaskPage';
import { useAppSelector } from '../hooks/useAppSelector';

import { ROLES } from '../constants/role.constants';

const AppRoutes = () => {
  return (
    <Routes>
      {/* Public */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/unauthorized" element={<UnauthorizedPage />} />

      {/* Protected — all routes inside require authentication */}
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>

          {/* Dashboard — Admin and Manager only */}
          <Route
            element={<RoleBasedRoute allowedRoles={[ROLES.ADMIN, ROLES.MANAGER]} />}
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
                allowedRoles={[ROLES.ADMIN, ROLES.MANAGER, ROLES.EMPLOYEE, ROLES.SUPPORT_USER]}
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
          <Route element={<RoleBasedRoute allowedRoles={[ROLES.SUPPORT_USER]} />}>
            <Route path="/my-tasks" element={<SupportTaskPage />} />
          </Route>

          {/* Default redirect */}
          <Route path="/" element={<DefaultRedirect />} />
          <Route path="*" element={<Navigate to="/" replace />} />

        </Route>
      </Route>
    </Routes>
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
    <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '100vh' }}>
      <h2>403 — Access Denied</h2>
      <p>You do not have permission to view this page.</p>
    </div>
  );
};

export default AppRoutes;