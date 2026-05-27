import { Box, Button, Tooltip } from "@mui/material";
import { useAppSelector } from "../../../hooks/useAppSelector";
import { type RequestDetailDto } from "../../../types/request.types";
import {
  STATUS_ENUM_REVERSE,
  STATUS,
} from "../../../constants/status.constants";
import { ROLES } from "../../../constants/role.constants";

interface RequestActionButtonsProps {
  request: RequestDetailDto;
  onApprove?: () => void;
  onReject?: () => void;
  onAssign?: () => void;
  onUpdateStatus?: () => void;
  onClose?: () => void;
  onEscalate?: () => void;
  onReopen?: () => void;
}

const RequestActionButtons = ({
  request,
  onApprove,
  onReject,
  onAssign,
  onUpdateStatus,
  onClose,
  onEscalate,
  onReopen,
}: RequestActionButtonsProps) => {
  const { roleName, userId } = useAppSelector((state) => state.auth);
  const { status, assignedTo, employeeId, isEscalated } = request;

  const isAdmin = roleName === ROLES.ADMIN;
  const isManager = roleName === ROLES.MANAGER;
  const isSupport = roleName === ROLES.SUPPORT_USER;
  const isEmployee = roleName === ROLES.EMPLOYEE;

  const statusLabel = STATUS_ENUM_REVERSE[status];

  const canApprove = isManager && statusLabel === STATUS.PENDING_APPROVAL;
  const canAssign =
    isAdmin && (statusLabel === STATUS.OPEN || statusLabel === STATUS.APPROVED);
  const showDisabledAssign = isAdmin && statusLabel === STATUS.PENDING_APPROVAL;
  const canUpdateStatus =
    isSupport &&
    assignedTo === userId &&
    (statusLabel === STATUS.ASSIGNED || statusLabel === STATUS.IN_PROGRESS);
  const canClose =
    statusLabel === STATUS.RESOLVED &&
    (isAdmin || (isEmployee && employeeId === userId));
  const canEscalate =
    isAdmin &&
    statusLabel !== STATUS.CLOSED &&
    statusLabel !== STATUS.REJECTED &&
    !isEscalated;
  const canReopen =
    isEmployee && employeeId === userId && statusLabel === STATUS.REJECTED;

  if (
    !canApprove &&
    !canAssign &&
    !showDisabledAssign &&
    !canUpdateStatus &&
    !canClose &&
    !canEscalate &&
    !canReopen
  ) {
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
      {showDisabledAssign && (
        <Tooltip title="Waiting for manager approval before this request can be assigned.">
          <span>
            <Button variant="contained" color="primary" size="small" disabled>
              Assign Support
            </Button>
          </span>
        </Tooltip>
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
      {canEscalate && (
        <Button
          variant="contained"
          color="error"
          size="small"
          onClick={onEscalate}
        >
          Escalate
        </Button>
      )}
      {canReopen && (
        <Button
          variant="contained"
          color="primary"
          size="small"
          onClick={onReopen}
        >
          Reopen Request
        </Button>
      )}
    </Box>
  );
};

export default RequestActionButtons;
