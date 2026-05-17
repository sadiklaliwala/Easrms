import { Box } from "@mui/material";
import { ReactNode } from "react";

interface AppFilterBarProps {
  children: ReactNode;
}

const AppFilterBar = ({ children }: AppFilterBarProps) => {
  return (
    <Box
      display="flex"
      flexWrap="wrap"
      gap={2}
      alignItems="center"
      mb={2}
      p={2}
      bgcolor="background.paper"
      borderRadius={1}
      border={1}
      borderColor="divider"
    >
      {children}
    </Box>
  );
};

export default AppFilterBar;