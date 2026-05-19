import { Box, Typography } from "@mui/material";
import FolderOpenIcon from "@mui/icons-material/FolderOpen";

interface AppEmptyStateProps {
  message?: string;
  subtitle?: string;
}

const AppEmptyState = ({
  message = "No records found",
  subtitle = "There are currently no items available to display.",
}: AppEmptyStateProps) => {
  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        py: 6,
        px: 3,
        textAlign: "center",
        width: "100%",
      }}
    >
      <Box
        sx={{
          width: 56,
          height: 56,
          borderRadius: "50%",
          bgcolor: "grey.100",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          color: "text.secondary",
          mb: 2,
        }}
      >
        <FolderOpenIcon sx={{ fontSize: 28 }} />
      </Box>
      <Typography variant="subtitle2" sx={{ fontWeight: 600, color: "text.primary", mb: 0.5 }}>
        {message}
      </Typography>
      <Typography variant="caption" sx={{ color: "text.secondary", maxWidth: 280 }}>
        {subtitle}
      </Typography>
    </Box>
  );
};

export default AppEmptyState;
