import { Chip } from "@mui/material";
import type { PriorityType } from "../../../../constants/priority.constants";

interface AppPriorityBadgeProps {
  priority: PriorityType | string;
}

const priorityColorMap: Record<
  string,
  "default" | "warning" | "error" | "success"
> = {
  Low: "success",
  Medium: "warning",
  High: "error",
};

const AppPriorityBadge = ({ priority }: AppPriorityBadgeProps) => {
  return (
    <Chip
      label={priority}
      color={priorityColorMap[priority] ?? "default"}
      size="small"
      variant="outlined"
    />
  );
};

export default AppPriorityBadge;
