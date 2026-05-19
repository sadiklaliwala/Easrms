import { Box, Button } from "@mui/material";
import { useAppSelector } from "../../../hooks/useAppSelector";
import { type RequestDetailDto } from "../../../types/request.types";
import { STATUS_ENUM_REVERSE, STATUS } from "../../../constants/status.constants";
import { ROLES } from "../../../constants/role.constants";

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

  const isAdmin = roleName === ROLES.ADMIN;
  const isManager = roleName === ROLES.MANAGER;
  const isSupport = roleName === ROLES.SUPPORT_USER;
  const isEmployee = roleName === ROLES.EMPLOYEE;

  const statusLabel = STATUS_ENUM_REVERSE[status];

  const canApprove = isManager && statusLabel === STATUS.PENDING_APPROVAL;
  const canAssign =
    isAdmin && (statusLabel === STATUS.OPEN || statusLabel === STATUS.APPROVED);
  console.log(assignedTo, userId, statusLabel, "assignedTo , userId");
  const canUpdateStatus =
    isSupport &&
    assignedTo === userId &&
    (statusLabel === STATUS.ASSIGNED || statusLabel === STATUS.IN_PROGRESS);
  const canClose =
    statusLabel === STATUS.RESOLVED &&
    (isAdmin || (isEmployee && employeeId === userId));

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
