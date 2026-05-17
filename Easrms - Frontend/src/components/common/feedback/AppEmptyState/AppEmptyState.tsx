import { Box, Typography } from "@mui/material";
import InboxIcon from "@mui/icons-material/Inbox";

interface AppEmptyStateProps {
  message?: string;
}

const AppEmptyState = ({ message = "No records found" }: AppEmptyStateProps) => {
  return (
    <Box display="flex" flexDirection="column" alignItems="center" justifyContent="center" minHeight="200px" gap={1}>
      <InboxIcon sx={{ fontSize: 48, color: "text.disabled" }} />
      <Typography variant="body2" color="text.secondary">
        {message}
      </Typography>
    </Box>
  );
};

export default AppEmptyState;