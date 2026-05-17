import { Box, Toolbar } from "@mui/material";
import { useState } from "react";
import { Outlet, useNavigate } from "react-router-dom";
import { AppTopbar } from "../AppTopbar";
import { AppSidebar } from "../AppSidebar";
import { useLogoutMutation } from "../../../../store/api/auth.endpoints";
import toast from "react-hot-toast";

const DRAWER_WIDTH = 240;

const AppLayout = () => {
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const navigate = useNavigate();
  const [logout] = useLogoutMutation();

  const handleLogout = async () => {
    try {
      await logout().unwrap();
    } catch {
      // ignore
    } finally {
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