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
    <Card elevation={1}>
      <CardContent>
        <Box
          sx={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
          }}
        >
          <Box>
            <Typography variant="body2" color="text.secondary" gutterBottom>
              {title}
            </Typography>
            <Typography variant="h4" sx={{ fontWeight: 700 }}>
              {count}
            </Typography>
          </Box>
          <Box
            sx={{
              bgcolor: `${color}20`,
              borderRadius: 2,
              p: 1.5,
              color: color,
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
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
