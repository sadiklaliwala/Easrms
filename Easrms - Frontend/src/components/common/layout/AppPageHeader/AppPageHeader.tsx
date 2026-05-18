import { Box, Typography } from "@mui/material";
import { type ReactNode } from "react";

interface AppPageHeaderProps {
  title: string;
  subtitle?: string;
  actions?: ReactNode;
}

const AppPageHeader = ({ title, subtitle, actions }: AppPageHeaderProps) => {
  return (
    <Box
      sx={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        mb: 3,
      }}
    >
      <Box>
        <Typography
          variant="h5"
          sx={{
            fontWeight: 600,
          }}
        >
          {title}
        </Typography>
        {subtitle && (
          <Typography
            variant="body2"
            sx={{
              color: "text.secondary",
              mt: 0.5,
            }}
          >
            {subtitle}
          </Typography>
        )}
      </Box>
      {actions && <Box>{actions}</Box>}
    </Box>
  );
};

export default AppPageHeader;
