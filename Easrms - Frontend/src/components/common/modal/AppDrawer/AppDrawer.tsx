import { Box, Drawer, IconButton, Typography } from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import { type ReactNode } from "react";

interface AppDrawerProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  anchor?: "left" | "right" | "top" | "bottom";
  width?: number;
}

const AppDrawer = ({
  open,
  onClose,
  title,
  children,
  anchor = "right",
  width = 400,
}: AppDrawerProps) => {
  return (
    <Drawer anchor={anchor} open={open} onClose={onClose}>
      <Box sx={{ width }} role="presentation">
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            px: 2,
            py: 1.5,
            borderBottom: 1,
            borderColor: "divider",
          }}
        >
          <Typography
            variant="h6"
            sx={{
              fontWeight: 600,
            }}
          >
            {title}
          </Typography>
          <IconButton onClick={onClose} size="small">
            <CloseIcon />
          </IconButton>
        </Box>
        <Box
          sx={{
            p: 2,
          }}
        >
          {children}
        </Box>
      </Box>
    </Drawer>
  );
};

export default AppDrawer;
