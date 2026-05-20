import { Box, Card, CardContent, Typography, Divider } from "@mui/material";

interface AppSLACardProps {
  withinSLA: number;
  nearingBreach: number;
  breached: number;
  escalated: number;
}

const AppSLACard = ({
  withinSLA,
  nearingBreach,
  breached,
  escalated,
}: AppSLACardProps) => {
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
        <Typography variant="body2" sx={{ color: "text.secondary", fontWeight: 600, mb: 2 }}>
          SLA Summary
        </Typography>
        
        <Box sx={{ display: "flex", justifyContent: "space-between", mb: 1 }}>
          <Typography variant="body2">Within SLA</Typography>
          <Typography variant="body2" sx={{ color: "success.main", fontWeight: 600 }}>{withinSLA}</Typography>
        </Box>
        <Divider sx={{ my: 1 }} />
        
        <Box sx={{ display: "flex", justifyContent: "space-between", mb: 1 }}>
          <Typography variant="body2">Nearing Breach</Typography>
          <Typography variant="body2" sx={{ color: "warning.main", fontWeight: 600 }}>{nearingBreach}</Typography>
        </Box>
        <Divider sx={{ my: 1 }} />
        
        <Box sx={{ display: "flex", justifyContent: "space-between", mb: 1 }}>
          <Typography variant="body2">Breached</Typography>
          <Typography variant="body2" sx={{ color: "error.main", fontWeight: 600 }}>{breached}</Typography>
        </Box>
        <Divider sx={{ my: 1 }} />
        
        <Box sx={{ display: "flex", justifyContent: "space-between" }}>
          <Typography variant="body2">Escalated</Typography>
          <Typography variant="body2" sx={{ color: "secondary.main", fontWeight: 600 }}>{escalated}</Typography>
        </Box>
      </CardContent>
    </Card>
  );
};

export default AppSLACard;
