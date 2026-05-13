import {
  Box,
  Drawer,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Typography,
  Divider,
} from "@mui/material";
import DashboardIcon from "@mui/icons-material/Dashboard";
import ListAltIcon from "@mui/icons-material/ListAlt";
import CategoryIcon from "@mui/icons-material/Category";
import PeopleIcon from "@mui/icons-material/People";
import ApprovalIcon from "@mui/icons-material/HowToReg";
import AssignmentIcon from "@mui/icons-material/AssignmentInd";
import SupportIcon from "@mui/icons-material/HeadsetMic";
import { useNavigate, useLocation } from "react-router-dom";
import { useAppSelector } from "../../../../hooks/useAppSelector";

interface AppSidebarProps {
  width: number;
  topbarHeight: number;
}

interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
  roles: string[];
}

const navItems: NavItem[] = [
  {
    label: "Dashboard",
    path: "/dashboard",
    icon: <DashboardIcon />,
    roles: ["Admin", "Manager", "Employee", "Support User"],
  },
  {
    label: "Requests",
    path: "/requests",
    icon: <ListAltIcon />,
    roles: ["Admin", "Manager", "Employee", "Support User"],
  },
  {
    label: "Approval",
    path: "/approval",
    icon: <ApprovalIcon />,
    roles: ["Manager"],
  },
  {
    label: "Assignment",
    path: "/assignment",
    icon: <AssignmentIcon />,
    roles: ["Admin"],
  },
  {
    label: "My Tasks",
    path: "/support",
    icon: <SupportIcon />,
    roles: ["Support User"],
  },
  {
    label: "Categories",
    path: "/categories",
    icon: <CategoryIcon />,
    roles: ["Admin"],
  },
  { label: "Users", path: "/users", icon: <PeopleIcon />, roles: ["Admin"] },
];

const AppSidebar = ({ width, topbarHeight }: AppSidebarProps) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAppSelector((state) => state.auth);

  const filteredNav = navItems.filter(
    (item) => user?.roleName && item.roles.includes(user.roleName),
  );

  return (
    <Drawer
      variant="permanent"
      sx={{
        width,
        flexShrink: 0,
        "& .MuiDrawer-paper": {
          width,
          boxSizing: "border-box",
          top: `${topbarHeight}px`,
          height: `calc(100% - ${topbarHeight}px)`,
          borderRight: "1px solid",
          borderColor: "divider",
          bgcolor: "background.paper",
        },
      }}
    >
      <Box sx={{ overflow: "auto", mt: 1 }}>
        <List disablePadding>
          {filteredNav.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <ListItemButton
                key={item.path}
                onClick={() => navigate(item.path)}
                sx={{
                  mx: 1,
                  mb: 0.5,
                  borderRadius: 2,
                  bgcolor: isActive ? "primary.main" : "transparent",
                  color: isActive ? "primary.contrastText" : "text.primary",
                  "&:hover": {
                    bgcolor: isActive ? "primary.dark" : "action.hover",
                  },
                  "& .MuiListItemIcon-root": {
                    color: isActive ? "primary.contrastText" : "text.secondary",
                    minWidth: 36,
                  },
                }}
              >
                <ListItemIcon>{item.icon}</ListItemIcon>
                <ListItemText
                  primary={item.label}
                  slotProps={{
                    primary: {
                      sx: {
                        fontSize: "0.875rem",
                        fontWeight: isActive ? 600 : 400,
                      },
                    },
                  }}
                />
              </ListItemButton>
            );
          })}
        </List>

        <Divider sx={{ mt: 2, mx: 2 }} />

        <Box sx={{ p: 2, mt: 1 }}>
          <Typography variant="caption" color="text.secondary">
            Logged in as
          </Typography>
          <Typography variant="body2" sx={{ fontWeight: 600 }}>
            {user?.fullName}
          </Typography>

          <Typography variant="caption" color="text.secondary">
            {user?.roleName}
          </Typography>
        </Box>
      </Box>
    </Drawer>
  );
};

export default AppSidebar;
