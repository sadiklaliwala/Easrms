import { Chip } from "@mui/material";
import { SLA_STATUS_COLOR } from "../../../../constants/sla.constants";

interface AppSLABadgeProps {
  slaStatus: string;
}

const getStatusStyles = (status: string) => {
  const color = SLA_STATUS_COLOR[status] || "default";
  if (color === "default") {
    return { bgcolor: "grey.100", color: "grey.700" };
  }
  return { bgcolor: `${color}.light`, color: `${color}.dark` };
};

const AppSLABadge = ({ slaStatus }: AppSLABadgeProps) => {
  const styles = getStatusStyles(slaStatus);
  return (
    <Chip
      label={slaStatus}
      size="small"
      sx={{
        bgcolor: styles.bgcolor,
        color: styles.color,
        fontWeight: 600,
        fontSize: "0.75rem",
        borderRadius: "6px",
        height: 22,
        border: "1px solid transparent",
      }}
    />
  );
};

export default AppSLABadge;
