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
import { useAppSelector } from "../../../../hooks/useAppSelector";
import { ROLES } from "../../../../constants/role.constants";

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
    roles: [ROLES.ADMIN, ROLES.MANAGER],
  },
  {
    label: "Users",
    path: "/users",
    icon: <PeopleIcon />,
    roles: [ROLES.ADMIN],
  },
  {
    label: "Categories",
    path: "/categories",
    icon: <CategoryIcon />,
    roles: [ROLES.ADMIN],
  },
  {
    label: "All Requests",
    path: "/requests",
    icon: <AssignmentIcon />,
    roles: [ROLES.ADMIN, ROLES.MANAGER],
  },
  {
    label: "My Requests",
    path: "/requests",
    icon: <AssignmentIcon />,
    roles: [ROLES.EMPLOYEE],
  },
  {
    label: "Approval Queue",
    path: "/approvals",
    icon: <CheckCircleIcon />,
    roles: [ROLES.MANAGER],
  },
  {
    label: "Assignment",
    path: "/assignments",
    icon: <AssignmentTurnedInIcon />,
    roles: [ROLES.ADMIN],
  },
  {
    label: "My Tasks",
    path: "/my-tasks",
    icon: <TaskIcon />,
    roles: [ROLES.SUPPORT_USER],
  },
];

const AppSidebar = ({ open }: AppSidebarProps) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { roleName } = useAppSelector((state) => state.auth);

  const visibleItems = NAV_ITEMS.filter((item) =>
    item.roles.includes(roleName ?? ""),
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
                    <Typography
                      variant="body2"
                      sx={{
                        fontWeight: isActive ? 600 : 400,
                      }}
                    >
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
