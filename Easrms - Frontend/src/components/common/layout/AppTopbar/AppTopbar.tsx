import {
  AppBar,
  Toolbar,
  Typography,
  Box,
  IconButton,
  Tooltip,
} from "@mui/material";
import LogoutIcon from "@mui/icons-material/Logout";
import {
  useAppDispatch,
  useAppSelector,
} from "../../../../hooks/useAppSelector";
import { clearCredentials } from "../../../../store/slices/authSlice";
import { useNavigate } from "react-router-dom";

interface AppTopbarProps {
  height: number;
  sidebarWidth: number;
}

const AppTopbar = ({ height, sidebarWidth }: AppTopbarProps) => {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { user } = useAppSelector((state) => state.auth);

  const handleLogout = () => {
    dispatch(clearCredentials());
    navigate("/login");
  };

  return (
    <AppBar
      position="fixed"
      elevation={0}
      sx={{
        width: `calc(100% - ${sidebarWidth}px)`,
        ml: `${sidebarWidth}px`,
        height,
        bgcolor: "background.paper",
        borderBottom: "1px solid",
        borderColor: "divider",
        color: "text.primary",
        justifyContent: "center",
      }}
    >
      <Toolbar sx={{ minHeight: `${height}px !important`, px: 3 }}>
        <Typography variant="h6" sx={{ flexGrow: 1, fontWeight: 600 }}>
          EASRMS
        </Typography>

        <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
          <Typography variant="body2" color="text.secondary">
            {user?.email}
          </Typography>

          <Tooltip title="Logout">
            <IconButton onClick={handleLogout} size="small" color="inherit">
              <LogoutIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default AppTopbar;
