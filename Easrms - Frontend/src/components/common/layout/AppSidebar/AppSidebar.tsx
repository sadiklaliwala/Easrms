import {
  Box,
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
} from "@mui/material";
import DashboardIcon from "@mui/icons-material/Dashboard";
import PeopleIcon from "@mui/icons-material/People";
import CategoryIcon from "@mui/icons-material/Category";
import AssignmentIcon from "@mui/icons-material/Assignment";
import AssignmentTurnedInIcon from "@mui/icons-material/AssignmentTurnedIn";
import TaskIcon from "@mui/icons-material/Task";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import { useNavigate, useLocation } from "react-router-dom";
import { useAppSelector } from "../../../hooks/useAppSelector";

const DRAWER_WIDTH = 240;

interface AppSidebarProps {
  open: boolean;
}

interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
  roles: string[];
}

const NAV_ITEMS: NavItem[] = [
  {
    label: "Dashboard",
    path: "/dashboard",
    icon: <DashboardIcon />,
    roles: ["Admin", "Manager"],
  },
  {
    label: "Users",
    path: "/users",
    icon: <PeopleIcon />,
    roles: ["Admin"],
  },
  {
    label: "Categories",
    path: "/categories",
    icon: <CategoryIcon />,
    roles: ["Admin"],
  },
  {
    label: "All Requests",
    path: "/requests",
    icon: <AssignmentIcon />,
    roles: ["Admin", "Manager"],
  },
  {
    label: "My Requests",
    path: "/requests",
    icon: <AssignmentIcon />,
    roles: ["Employee"],
  },
  {
    label: "Approval Queue",
    path: "/approval",
    icon: <CheckCircleIcon />,
    roles: ["Manager"],
  },
  {
    label: "Assignment",
    path: "/assignment",
    icon: <AssignmentTurnedInIcon />,
    roles: ["Admin"],
  },
  {
    label: "My Tasks",
    path: "/support",
    icon: <TaskIcon />,
    roles: ["Support User"],
  },
];

const AppSidebar = ({ open }: AppSidebarProps) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { roleName } = useAppSelector((state) => state.auth);

  const visibleItems = NAV_ITEMS.filter((item) =>
    item.roles.includes(roleName ?? "")
  );

  return (
    <Drawer
      variant="persistent"
      open={open}
      sx={{
        width: DRAWER_WIDTH,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width: DRAWER_WIDTH,
          boxSizing: "border-box",
        },
      }}
    >
      <Toolbar />
      <Box sx={{ overflow: "auto", mt: 1 }}>
        <List dense>
          {visibleItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <ListItemButton
                key={item.label}
                selected={isActive}
                onClick={() => navigate(item.path)}
                sx={{
                  mx: 1,
                  borderRadius: 1,
                  mb: 0.5,
                  "&.Mui-selected": {
                    bgcolor: "primary.main",
                    color: "white",
                    "& .MuiListItemIcon-root": { color: "white" },
                    "&:hover": { bgcolor: "primary.dark" },
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 36 }}>{item.icon}</ListItemIcon>
                <ListItemText
                  primary={
                    <Typography variant="body2" fontWeight={isActive ? 600 : 400}>
                      {item.label}
                    </Typography>
                  }
                />
              </ListItemButton>
            );
          })}
        </List>
      </Box>
    </Drawer>
  );
};

export default AppSidebar;