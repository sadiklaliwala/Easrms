import { Box } from "@mui/material";
import { type ReactNode } from "react";

interface AppFilterBarProps {
  children: ReactNode;
}

const AppFilterBar = ({ children }: AppFilterBarProps) => {
  return (
    <Box
      sx={{
        display: "flex",
        // flexWrap: "wrap",
        flexWrap: "nowrap",
        overflow: "auto",
        gap: 2,
        alignItems: "center",
        mb: 2,
        p: 2,
        bgcolor: "background.paper",
        borderRadius: 1,
        border: 1,
        borderColor: "divider",
      }}
    >
      {children}
    </Box>
  );
};

export default AppFilterBar;
