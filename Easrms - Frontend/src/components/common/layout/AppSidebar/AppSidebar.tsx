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
import BusinessCenterIcon from "@mui/icons-material/BusinessCenter";
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
    roles: [ROLES.ADMIN, ROLES.MANAGER, ROLES.EMPLOYEE],
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
        width: open ? DRAWER_WIDTH : 0,
        flexShrink: 0,
        transition: (theme) =>
          theme.transitions.create("width", {
            easing: open
              ? theme.transitions.easing.easeOut
              : theme.transitions.easing.sharp,
            duration: open
              ? theme.transitions.duration.enteringScreen
              : theme.transitions.duration.leavingScreen,
          }),
        "& .MuiDrawer-paper": {
          width: DRAWER_WIDTH,
          boxSizing: "border-box",
          borderRight: "1px solid",
          borderColor: "divider",
          bgcolor: "background.paper",
        },
      }}
    >
      <Toolbar />
      <Box
        sx={{
          px: 2.5,
          py: 2,
          display: "flex",
          alignItems: "center",
          gap: 1.5,
          borderBottom: "1px solid",
          borderColor: "divider",
        }}
      >
        <Box
          sx={{
            bgcolor: "rgba(79, 70, 229, 0.08)",
            color: "secondary.main",
            width: 36,
            height: 36,
            borderRadius: 2,
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            flexShrink: 0,
          }}
        >
          <BusinessCenterIcon sx={{ fontSize: 20 }} />
        </Box>
        <Box>
          <Typography
            variant="subtitle2"
            sx={{ fontWeight: 700, color: "text.primary", lineHeight: 1.2 }}
          >
            EASRMS Portal
          </Typography>
          <Typography
            variant="caption"
            sx={{ color: "text.secondary", fontWeight: 600 }}
          >
            v1.0.0
          </Typography>
        </Box>
      </Box>
      <Box sx={{ overflow: "auto", mt: 1 }}>
        <List sx={{ px: 1 }}>
          {visibleItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <ListItemButton
                key={item.label}
                selected={isActive}
                onClick={() => navigate(item.path)}
                sx={{
                  borderRadius: 1.5,
                  mb: 0.5,
                  py: 1,
                  px: 2,
                  color: "text.secondary",
                  transition: "all 0.2s ease-in-out",
                  "& .MuiListItemIcon-root": {
                    color: "text.secondary",
                    minWidth: 32,
                    transition: "color 0.2s ease-in-out",
                  },
                  "&.Mui-selected": {
                    bgcolor: "rgba(79, 70, 229, 0.08)",
                    color: "secondary.main",
                    fontWeight: 600,
                    "& .MuiListItemIcon-root": { color: "secondary.main" },
                    "&:hover": {
                      bgcolor: "rgba(79, 70, 229, 0.12)",
                    },
                  },
                  "&:hover:not(.Mui-selected)": {
                    bgcolor: "grey.100",
                    color: "text.primary",
                    "& .MuiListItemIcon-root": { color: "text.primary" },
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 32 }}>{item.icon}</ListItemIcon>
                <ListItemText
                  primary={
                    <Typography
                      variant="body2"
                      sx={{
                        fontWeight: isActive ? 600 : 500,
                        letterSpacing: "-0.01em",
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
