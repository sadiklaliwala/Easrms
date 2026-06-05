// import { Navigate, Outlet } from 'react-router-dom';
// import { useAppSelector } from '../hooks/useAppSelector';

// const ProtectedRoute = () => {
//   const { isAuthenticated } = useAppSelector((state) => state.auth);

//   return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
// };

// export default ProtectedRoute;

import { Navigate, Outlet } from "react-router-dom";
import { useAppSelector } from "../hooks/useAppSelector";
import AppLoader from "../components/common/feedback/AppLoader";

const ProtectedRoute = () => {
  const { isAuthenticated, isInitializing } = useAppSelector(
    (state) => state.auth,
  );

  // Show loader ONLY when we're waiting for the /me token check on app refresh
  // and we don't yet know if the user is authenticated.
  // If isAuthenticated is already true (e.g. just logged in), skip the loader.
  if (isInitializing && !isAuthenticated) return <AppLoader />;

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
