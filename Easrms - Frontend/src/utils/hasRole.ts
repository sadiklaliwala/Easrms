import { ROLES, type RoleType } from "../constants/role.constants";

export const hasRole = (
  currentRole: string | null | undefined,
  ...allowedRoles: RoleType[]
): boolean => {
  if (!currentRole) return false;
  return allowedRoles.includes(currentRole as RoleType);
};

export const isAdmin = (role: string | null | undefined) =>
  hasRole(role, ROLES.ADMIN);

export const isManager = (role: string | null | undefined) =>
  hasRole(role, ROLES.MANAGER);

export const isEmployee = (role: string | null | undefined) =>
  hasRole(role, ROLES.EMPLOYEE);

export const isSupportUser = (role: string | null | undefined) =>
  hasRole(role, ROLES.SUPPORT_USER);