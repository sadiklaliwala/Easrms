import { Chip } from "@mui/material";

interface AppStatusBadgeProps {
  status: string;
}

const statusColorMap: Record<string, "default" | "warning" | "info" | "success" | "error" | "primary" | "secondary"> = {
  Open: "info",
  "Pending Approval": "warning",
  Approved: "primary",
  Rejected: "error",
  Assigned: "secondary",
  "In Progress": "warning",
  Resolved: "success",
  Closed: "default",
};

const AppStatusBadge = ({ status }: AppStatusBadgeProps) => {
  return (
    <Chip
      label={status}
      color={statusColorMap[status] ?? "default"}
      size="small"
      variant="filled"
    />
  );
};

export default AppStatusBadge;