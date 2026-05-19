import { Box, Typography } from "@mui/material";
import { ErrorOutlined } from "@mui/icons-material";

interface AppErrorStateProps {
  message?: string;
}

const AppErrorState = ({
  message = "Something went wrong",
}: AppErrorStateProps) => {
  return (
    <Box
      sx={{
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        justifyContent: "center",
        minHeight: "200px",
        gap: 1,
      }}
    >
      <ErrorOutlined sx={{ fontSize: 48, color: "error.main" }} />
      <Typography variant="body2" color="error">
        {message}
      </Typography>
    </Box>
  );
};

export default AppErrorState;
