import { Routes, Route, Navigate } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
import RoleBasedRoute from './RoleBasedRoute';
import LoginPage from '../pages/auth/LoginPage';
import DashboardPage from '../pages/dashboard/DashboardPage';
import RequestListPage from '../pages/requests/RequestListPage';
import RequestDetailPage from '../pages/requests/RequestDetailPage';
import CreateRequestPage from '../pages/requests/CreateRequestPage';
import CategoryPage from '../pages/categories/CategoryPage';
import UserPage from '../pages/users/UserPage';
import ApprovalQueuePage from '../pages/approval/ApprovalQueuePage';
import AssignmentPage from '../pages/assignment/AssignmentPage';
import SupportTaskPage from '../pages/support/SupportTaskPage';
import NotFound from '../pages/NotFound';
import { RoleConstants } from '../constants/role.constants';

const AppRoutes = () => {
  return (
    <Routes>

      {/* Public */}
      <Route path="/login" element={<LoginPage />} />

      {/* Protected Routes */}
      <Route element={<ProtectedRoute />}>

        {/* All Roles */}
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/requests" element={<RequestListPage />} />
        <Route path="/requests/:id" element={<RequestDetailPage />} />

        {/* Employee Only */}
        <Route element={<RoleBasedRoute allowedRoles={[RoleConstants.Employee]} />}>
          <Route path="/requests/create" element={<CreateRequestPage />} />
        </Route>

        {/* Admin Only */}
        <Route element={<RoleBasedRoute allowedRoles={[RoleConstants.Admin]} />}>
          <Route path="/categories" element={<CategoryPage />} />
          <Route path="/users" element={<UserPage />} />
          <Route path="/assignment" element={<AssignmentPage />} />
        </Route>

        {/* Manager Only */}
        <Route element={<RoleBasedRoute allowedRoles={[RoleConstants.Manager]} />}>
          <Route path="/approval" element={<ApprovalQueuePage />} />
        </Route>

        {/* Support User Only */}
        <Route element={<RoleBasedRoute allowedRoles={[RoleConstants.SupportUser]} />}>
          <Route path="/support/tasks" element={<SupportTaskPage />} />
        </Route>

      </Route>

      {/* Default */}
      <Route path="/" element={<Navigate to="/login" />} />
      <Route path="*" element={<NotFound />} />

    </Routes>
  );
};

export default AppRoutes;