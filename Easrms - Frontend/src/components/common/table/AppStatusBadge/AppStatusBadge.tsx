import { Chip } from "@mui/material";
import { STATUS } from "../../../../constants/status.constants";
import type { StatusType } from "../../../../constants/status.constants";

interface AppStatusBadgeProps {
  status: StatusType | string;
}

const getStatusStyles = (status: string) => {
  switch (status) {
    case STATUS.OPEN:
      return { bgcolor: "info.light", color: "info.dark" };
    case STATUS.PENDING_APPROVAL:
      return { bgcolor: "warning.light", color: "warning.dark" };
    case STATUS.APPROVED:
      return { bgcolor: "success.light", color: "success.dark" };
    case STATUS.REJECTED:
      return { bgcolor: "error.light", color: "error.dark" };
    case STATUS.ASSIGNED:
      return { bgcolor: "rgba(79, 70, 229, 0.08)", color: "secondary.dark" };
    case STATUS.IN_PROGRESS:
      return { bgcolor: "warning.light", color: "warning.dark" };
    case STATUS.RESOLVED:
      return { bgcolor: "success.light", color: "success.dark" };
    case STATUS.CLOSED:
    default:
      return { bgcolor: "grey.100", color: "grey.700" };
  }
};

const AppStatusBadge = ({ status }: AppStatusBadgeProps) => {
  const styles = getStatusStyles(status);
  return (
    <Chip
      label={status}
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

export default AppStatusBadge;
