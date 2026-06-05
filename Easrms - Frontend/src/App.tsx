import AppRoutes from "./routes/AppRoutes";
import AppToast from "./components/common/feedback/AppToast";
import { useAppDispatch } from "./hooks/useAppSelector";
import { useGetMeQuery } from "./store/api/auth.endpoints";
import { useEffect } from "react";
import { setCredentials, setInitializingDone } from "./store/slices/authSlice";

const AppInitializer = () => {
  const dispatch = useAppDispatch();
  const hasToken = !!localStorage.getItem("accessToken");

  // Always fire immediately — do NOT skip while initializing.
  // This runs in the background while the app already renders routes.
  const {
    data: response,
    isSuccess,
    isError,
  } = useGetMeQuery(undefined, {
    // Only make the network call if a token is present in storage
    skip: !hasToken,
    refetchOnMountOrArgChange: false,
    refetchOnFocus: false,
    refetchOnReconnect: false,
  });

  useEffect(() => {
    // No token — mark initializing done immediately so routes render
    if (!hasToken) {
      dispatch(setInitializingDone());
      return;
    }

    // Token exists: wait for the /me response
    if (isSuccess && response?.success && response.data) {
      dispatch(
        setCredentials({
          userId: response.data.userId,
          fullName: response.data.fullName,
          email: response.data.email,
          roleName: response.data.roleName,
          managerId: response.data.managerId ?? null,
          accessToken: "",
          refreshToken: "",
        }),
      );
    }

    if (isError) {
      // Token is invalid/expired — clear initializing so the user
      // gets redirected to /login by ProtectedRoute
      dispatch(setInitializingDone());
    }
  }, [isSuccess, isError, response, dispatch, hasToken]);

  // Always render routes — ProtectedRoute handles the loading state
  return <AppRoutes />;
};

const App = () => {
  return (
    <>
      <AppToast />
      <AppInitializer />
    </>
  );
};

export default App;
