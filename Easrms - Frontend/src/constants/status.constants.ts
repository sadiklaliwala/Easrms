export const STATUS = {
  OPEN: "Open",
  PENDING_APPROVAL: "Pending Approval",
  APPROVED: "Approved",
  REJECTED: "Rejected",
  ASSIGNED: "Assigned",
  IN_PROGRESS: "In Progress",
  RESOLVED: "Resolved",
  CLOSED: "Closed",
} as const;

export type StatusType = (typeof STATUS)[keyof typeof STATUS];

export const STATUS_OPTIONS = Object.values(STATUS).map((s) => ({
  label: s,
  value: s,
}));

export const STATUS_ENUM: Record<StatusType, number> = {
  [STATUS.OPEN]: 1,
  [STATUS.PENDING_APPROVAL]: 2,
  [STATUS.APPROVED]: 3,
  [STATUS.REJECTED]: 4,
  [STATUS.ASSIGNED]: 5,
  [STATUS.IN_PROGRESS]: 6,
  [STATUS.RESOLVED]: 7,
  [STATUS.CLOSED]: 8,
};

export const STATUS_ENUM_REVERSE: Record<number, StatusType> = {
  1: STATUS.OPEN,
  2: STATUS.PENDING_APPROVAL,
  3: STATUS.APPROVED,
  4: STATUS.REJECTED,
  5: STATUS.ASSIGNED,
  6: STATUS.IN_PROGRESS,
  7: STATUS.RESOLVED,
  8: STATUS.CLOSED,
};
