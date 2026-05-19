import { Box, Card, CardContent, Typography } from "@mui/material";
import { type ReactNode } from "react";

interface AppMetricCardProps {
  title: string;
  count: number;
  icon: ReactNode;
  color?: string;
}

const AppMetricCard = ({
  title,
  count,
  icon,
  color = "#1976d2",
}: AppMetricCardProps) => {
  return (
    <Card
      elevation={0}
      sx={{
        transition: "all 0.2s ease-in-out",
        "&:hover": {
          borderColor: "grey.300",
          transform: "translateY(-2px)",
        },
      }}
    >
      <CardContent sx={{ p: 2.5, "&:last-child": { pb: 2.5 } }}>
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Box>
            <Typography variant="body2" sx={{ color: "text.secondary", fontWeight: 600, mb: 0.5 }}>
              {title}
            </Typography>
            <Typography variant="h4" sx={{ fontWeight: 700, letterSpacing: "-0.02em" }}>
              {count}
            </Typography>
          </Box>
          <Box
            sx={{
              bgcolor: `${color}12`,
              borderRadius: 2.5,
              width: 44,
              height: 44,
              color: color,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              "& svg": {
                fontSize: "1.5rem",
              },
            }}
          >
            {icon}
          </Box>
        </Box>
      </CardContent>
    </Card>
  );
};

export default AppMetricCard;
