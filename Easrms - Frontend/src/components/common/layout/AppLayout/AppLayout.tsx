import { Box, Toolbar } from "@mui/material";
import { useState } from "react";
import { Outlet, useNavigate } from "react-router-dom";
import AppTopbar from "../AppTopbar";
import AppSidebar from "../AppSidebar";
import { useLogoutMutation } from "../../../../store/api/auth.endpoints";
import toast from "react-hot-toast";
import { useAppDispatch } from "../../../../hooks/useAppSelector";
import { clearCredentials } from "../../../../store/slices/authSlice";
import { api } from "../../../../store/api/api";

const DRAWER_WIDTH = 240;

const AppLayout = () => {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const navigate = useNavigate();
  const [logout] = useLogoutMutation();
  const dispatch = useAppDispatch();

  const handleLogout = async () => {
    try {
      await logout().unwrap();
      console.log("logged out and navigated");
      navigate("/login");
    } catch {
      // ignore
    } finally {
      dispatch(clearCredentials());
      dispatch(api.util.resetApiState());
      navigate("/login");
      toast.success("Logged out successfully");
    }
  };

  return (
    <Box sx={{ display: "flex" }}>
      <AppTopbar
        onMenuToggle={() => setSidebarOpen((prev) => !prev)}
        onLogout={handleLogout}
      />
      <AppSidebar open={sidebarOpen} />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          ml: sidebarOpen ? `${DRAWER_WIDTH}px` : 0,
          transition: "margin 0.2s ease",
          minHeight: "100vh",
          bgcolor: "grey.50",
        }}
      >
        <Toolbar />
        <Outlet />
      </Box>
    </Box>
  );
};

export default AppLayout;
