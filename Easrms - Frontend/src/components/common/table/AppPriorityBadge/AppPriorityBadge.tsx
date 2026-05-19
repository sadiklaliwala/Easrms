import { Chip } from "@mui/material";
import type { PriorityType } from "../../../../constants/priority.constants";

interface AppPriorityBadgeProps {
  priority: PriorityType | string;
}

const getPriorityStyles = (priority: string) => {
  switch (priority) {
    case "Low":
      return { bgcolor: "success.light", color: "success.dark" };
    case "Medium":
      return { bgcolor: "warning.light", color: "warning.dark" };
    case "High":
      return { bgcolor: "error.light", color: "error.dark" };
    default:
      return { bgcolor: "grey.100", color: "grey.700" };
  }
};

const AppPriorityBadge = ({ priority }: AppPriorityBadgeProps) => {
  const styles = getPriorityStyles(priority as string);
  return (
    <Chip
      label={priority}
      size="small"
      sx={{
        bgcolor: styles.bgcolor,
        color: styles.color,
        fontWeight: 600,
        fontSize: "0.75rem",
        borderRadius: "6px",
        height: 22,
      }}
    />
  );
};

export default AppPriorityBadge;
