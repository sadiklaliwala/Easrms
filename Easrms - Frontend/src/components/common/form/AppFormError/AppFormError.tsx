import { Typography } from "@mui/material";

interface AppFormErrorProps {
  message?: string;
}

const AppFormError = ({ message }: AppFormErrorProps) => {
  if (!message) return null;
  return (
    <Typography
      variant="caption"
      sx={{
        color: "error",
        mt: 0.5,
      }}
    >
      {message}
    </Typography>
  );
};

export default AppFormError;
