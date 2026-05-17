import { Chip } from "@mui/material";

interface AppPriorityBadgeProps {
  priority: string;
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
