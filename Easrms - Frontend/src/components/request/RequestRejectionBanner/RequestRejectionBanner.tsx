import { Alert, Button } from "@mui/material";
import { useAppSelector } from "../../../hooks/useAppSelector";
import { type RequestDetailDto } from "../../../types/request.types";
import { ROLES } from "../../../constants/role.constants";
import { STATUS_ENUM_REVERSE, STATUS } from "../../../constants/status.constants";

interface RequestRejectionBannerProps {
  reason: string;
  request: RequestDetailDto;
  onReopen: () => void;
}

const RequestRejectionBanner = ({
  reason,
  request,
  onReopen,
}: RequestRejectionBannerProps) => {
  const { roleName, userId } = useAppSelector((state) => state.auth);
  const statusLabel = STATUS_ENUM_REVERSE[request.status];

  const canReopen =
    roleName === ROLES.EMPLOYEE &&
    request.employeeId === userId &&
    statusLabel === STATUS.REJECTED;

  return (
    <Alert
      severity="error"
      sx={{
        mb: 2,
        display: "flex",
        alignItems: "center",
      }}
      action={
        canReopen ? (
          <Button
            color="error"
            size="small"
            variant="contained"
            onClick={onReopen}
            sx={{ textTransform: "none", fontWeight: 600 }}
          >
            Reopen Request
          </Button>
        ) : undefined
      }
    >
      <strong>Rejected:</strong> {reason}
    </Alert>
  );
};

export default RequestRejectionBanner;