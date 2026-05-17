import { Box, Typography } from "@mui/material";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";

interface AppErrorStateProps {
  message?: string;
}

const AppErrorState = ({ message = "Something went wrong" }: AppErrorStateProps) => {
  return (
    <Box display="flex" flexDirection="column" alignItems="center" justifyContent="center" minHeight="200px" gap={1}>
      <ErrorOutlineIcon sx={{ fontSize: 48, color: "error.main" }} />
      <Typography variant="body2" color="error">
        {message}
      </Typography>
    </Box>
  );
};

export default AppErrorState;