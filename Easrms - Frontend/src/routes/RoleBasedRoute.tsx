// import { Navigate, Outlet } from 'react-router-dom';
// import { useAppSelector } from '../hooks/useAppSelector';

// interface RoleBasedRouteProps {
//   allowedRoles: string[];
// }

// const RoleBasedRoute = ({ allowedRoles }: RoleBasedRouteProps) => {
//   const { user } = useAppSelector((state) => state.auth);

//   return user && allowedRoles.includes(user.roleName)
//     ? <Outlet />
//     : <Navigate to="/dashboard" replace />;
// };

// export default RoleBasedRoute;

import { Navigate, Outlet } from 'react-router-dom';
import { useAppSelector } from '../hooks/useAppSelector';

interface RoleBasedRouteProps {
  allowedRoles: string[];
}

const RoleBasedRoute = ({ allowedRoles }: RoleBasedRouteProps) => {
  const { roleName } = useAppSelector((state) => state.auth);

  if (!roleName || !allowedRoles.includes(roleName)) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <Outlet />;
};

export default RoleBasedRoute;