import { Box, Button } from "@mui/material";
import { useAppSelector } from "../../../hooks/useAppSelector";
import { type RequestDetailDto } from "../../../types/request.types";

interface RequestActionButtonsProps {
  request: RequestDetailDto;
  onApprove?: () => void;
  onReject?: () => void;
  onAssign?: () => void;
  onUpdateStatus?: () => void;
  onClose?: () => void;
}

const RequestActionButtons = ({
  request,
  onApprove,
  onReject,
  onAssign,
  onUpdateStatus,
  onClose,
}: RequestActionButtonsProps) => {
  const { roleName, userId } = useAppSelector((state) => state.auth);
  const { status, assignedTo, employeeId } = request as any;

  const isAdmin = roleName === "Admin";
  const isManager = roleName === "Manager";
  const isSupport = roleName === "Support User";
  const isEmployee = roleName === "Employee";

  const canApprove = isManager && status === "Pending Approval";
  const canAssign = isAdmin && (status === "Open" || status === "Approved");
  const canUpdateStatus =
    isSupport &&
    assignedTo === userId &&
    (status === "Assigned" || status === "In Progress");
  const canClose =
    status === "Resolved" && (isAdmin || (isEmployee && employeeId === userId));

  if (!canApprove && !canAssign && !canUpdateStatus && !canClose) {
    return null;
  }

  return (
    <Box
      sx={{
        display: "flex",
        gap: 1,
        flexWrap: "wrap",
      }}
    >
      {canApprove && (
        <>
          <Button
            variant="contained"
            color="success"
            size="small"
            onClick={onApprove}
          >
            Approve
          </Button>
          <Button
            variant="outlined"
            color="error"
            size="small"
            onClick={onReject}
          >
            Reject
          </Button>
        </>
      )}
      {canAssign && (
        <Button
          variant="contained"
          color="primary"
          size="small"
          onClick={onAssign}
        >
          Assign Support
        </Button>
      )}
      {canUpdateStatus && (
        <Button
          variant="contained"
          color="warning"
          size="small"
          onClick={onUpdateStatus}
        >
          Update Status
        </Button>
      )}
      {canClose && (
        <Button variant="outlined" color="error" size="small" onClick={onClose}>
          Close Request
        </Button>
      )}
    </Box>
  );
};

export default RequestActionButtons;
