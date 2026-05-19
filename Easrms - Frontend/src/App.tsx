// import AppRoutes from "./routes/AppRoutes";

// function App() {
//   return (
//     <>
//       <AppRoutes />
//     </>
//   );
// }

// export default App;

import AppRoutes from "./routes/AppRoutes";
import AppToast from "./components/common/feedback/AppToast";
import { useAppDispatch, useAppSelector } from "./hooks/useAppSelector";
import { useGetMeQuery } from "./store/api/auth.endpoints";
import { useEffect } from "react";
import { setCredentials, setInitializingDone } from "./store/slices/authSlice";

const AppInitializer = () => {
  const dispatch = useAppDispatch();
  const { isInitializing } = useAppSelector((state) => state.auth);

  const {
    data: response,
    isSuccess,
    isError,
  } = useGetMeQuery(undefined, {
    skip: !isInitializing, // only runs on first load
    refetchOnMountOrArgChange: false, // never refetch automatically
    refetchOnFocus: false, // do not refetch when tab regains focus
    refetchOnReconnect: false, // do not refetch on reconnect
  });

  useEffect(() => {
    if (isSuccess && response?.success && response.data) {
      dispatch(
        setCredentials({
          userId: response.data.userId,
          fullName: response.data.fullName,
          email: response.data.email,
          roleName: response.data.roleName,
          managerId: null,
          accessToken: "",
          refreshToken: "",
        }),
      );
    }

    if (isError) {
      dispatch(setInitializingDone());
    }
  }, [isSuccess, isError]);

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
