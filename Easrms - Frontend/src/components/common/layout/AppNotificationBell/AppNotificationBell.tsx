import { useState } from "react";
import {
  Badge,
  IconButton,
  Menu,
  MenuItem,
  Typography,
  Box,
  Divider,
  Button,
  List,
  ListItemText,
} from "@mui/material";
import NotificationsIcon from "@mui/icons-material/Notifications";
import {
  useAppDispatch,
  useAppSelector,
} from "../../../../hooks/useAppSelector";
import {
  markAsRead,
  markAllAsRead,
  clearNotifications,
} from "../../../../store/slices/notificationSlice";

const AppNotificationBell = () => {
  const dispatch = useAppDispatch();
  const { notifications, unreadCount } = useAppSelector(
    (state) => state.notification,
  );
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const handleOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleMarkAllRead = () => {
    dispatch(markAllAsRead());
  };

  const handleClearAll = () => {
    dispatch(clearNotifications());
    handleClose();
  };

  const handleNotificationClick = (id: string) => {
    dispatch(markAsRead(id));
  };

  return (
    <>
      <IconButton
        color="inherit"
        onClick={handleOpen}
        sx={{ color: "text.secondary" }}
      >
        <Badge badgeContent={unreadCount} color="error">
          <NotificationsIcon />
        </Badge>
      </IconButton>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleClose}
        slotProps={{
          paper: {
            sx: {
              width: 320,
              maxHeight: 400,
              boxShadow: "0px 8px 16px rgba(0, 0, 0, 0.1)",
              borderRadius: 2,
            },
          },
        }}
        anchorOrigin={{
          vertical: "bottom",
          horizontal: "right",
        }}
        transformOrigin={{
          vertical: "top",
          horizontal: "right",
        }}
      >
        <Box
          sx={{
            p: 2,
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
            Notifications
          </Typography>
          {unreadCount > 0 && (
            <Button
              size="small"
              onClick={handleMarkAllRead}
              sx={{ fontSize: "0.75rem" }}
            >
              Mark all as read
            </Button>
          )}
        </Box>
        <Divider />

        {notifications.length === 0 ? (
          <Box sx={{ p: 3, textAlign: "center" }}>
            <Typography variant="body2" color="text.secondary">
              No new notifications
            </Typography>
          </Box>
        ) : (
          <>
            <List sx={{ p: 0, overflowY: "auto", maxHeight: 280 }}>
              {notifications.map((notif) => (
                <MenuItem
                  key={notif.id}
                  onClick={() => handleNotificationClick(notif.id)}
                  sx={{
                    whiteSpace: "normal",
                    bgcolor: notif.isRead ? "transparent" : "action.hover",
                    borderLeft: notif.isRead ? "none" : "4px solid",
                    borderColor: "primary.main",
                    py: 1,
                  }}
                >
                  <ListItemText
                    primary={
                      <Typography
                        variant="body2"
                        sx={{ fontWeight: notif.isRead ? 400 : 600 }}
                      >
                        {notif.message}
                      </Typography>
                    }
                    secondary={
                      <Typography variant="caption" color="text.secondary">
                        {new Date(notif.createdAt).toLocaleTimeString([], {
                          hour: "2-digit",
                          minute: "2-digit",
                        })}
                      </Typography>
                    }
                  />
                </MenuItem>
              ))}
            </List>
            <Divider />
            <Box sx={{ p: 1, display: "flex", justifyContent: "center" }}>
              <Button
                size="small"
                color="error"
                onClick={handleClearAll}
                sx={{ fontSize: "0.75rem" }}
              >
                Clear all
              </Button>
            </Box>
          </>
        )}
      </Menu>
    </>
  );
};

export default AppNotificationBell;
