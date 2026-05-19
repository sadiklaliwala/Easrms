import { Chip } from "@mui/material";
import { STATUS } from "../../../../constants/status.constants";
import type { StatusType } from "../../../../constants/status.constants";

interface AppStatusBadgeProps {
  status: StatusType | string;
}

const statusColorMap: Record<
  string,
  "default" | "warning" | "info" | "success" | "error" | "primary" | "secondary"
> = {
  [STATUS.OPEN]: "info",
  [STATUS.PENDING_APPROVAL]: "warning",
  [STATUS.APPROVED]: "primary",
  [STATUS.REJECTED]: "error",
  [STATUS.ASSIGNED]: "secondary",
  [STATUS.IN_PROGRESS]: "warning",
  [STATUS.RESOLVED]: "success",
  [STATUS.CLOSED]: "default",
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
