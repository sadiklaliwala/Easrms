export const ROLES = {
  ADMIN: "Admin",
  MANAGER: "Manager",
  EMPLOYEE: "Employee",
  SUPPORT_USER: "Support",
} as const;

export type RoleType = (typeof ROLES)[keyof typeof ROLES];
