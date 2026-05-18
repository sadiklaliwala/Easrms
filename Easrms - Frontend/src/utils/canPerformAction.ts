import { ROLES } from "../constants/role.constants";
import { STATUS } from "../constants/status.constants";
import { STATUS_ENUM_REVERSE } from "../constants/status.constants";

export const canApproveOrReject = (
  roleName: string,
  status: number,
): boolean => {
  return (
    roleName === ROLES.MANAGER &&
    STATUS_ENUM_REVERSE[status] === STATUS.PENDING_APPROVAL
  );
};

export const canAssign = (roleName: string, status: number): boolean => {
  const statusLabel = STATUS_ENUM_REVERSE[status];
  return (
    roleName === ROLES.ADMIN &&
    (statusLabel === STATUS.OPEN || statusLabel === STATUS.APPROVED)
  );
};

export const canUpdateStatus = (
  roleName: string,
  status: number,
  assignedToUserId: string,
  currentUserId: string,
): boolean => {
  const statusLabel = STATUS_ENUM_REVERSE[status];
  return (
    roleName === ROLES.SUPPORT_USER &&
    assignedToUserId === currentUserId &&
    (statusLabel === STATUS.ASSIGNED || statusLabel === STATUS.IN_PROGRESS)
  );
};

export const canCloseRequest = (
  roleName: string,
  status: number,
  employeeId: string,
  currentUserId: string,
): boolean => {
  const statusLabel = STATUS_ENUM_REVERSE[status];
  return (
    statusLabel === STATUS.RESOLVED &&
    (roleName === ROLES.ADMIN ||
      (roleName === ROLES.EMPLOYEE && employeeId === currentUserId))
  );
};
