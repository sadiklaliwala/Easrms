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

  // Wait for getMe to finish before making auth decision
  if (isInitializing) return <AppLoader />;

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
};

export default ProtectedRoute;
