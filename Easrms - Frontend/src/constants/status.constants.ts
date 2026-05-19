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

export const STATUS_ENUM: Record<string, number> = {
  Open: 1,
  PendingApproval: 2,
  Approved: 3,
  Rejected: 4,
  Assigned: 5,
  InProgress: 6,
  Resolved: 7,
  Closed: 8,
};

export const STATUS_ENUM_REVERSE: Record<number, string> = {
  1: STATUS.OPEN,
  2: STATUS.PENDING_APPROVAL,
  3: STATUS.APPROVED,
  4: STATUS.REJECTED,
  5: STATUS.ASSIGNED,
  6: STATUS.IN_PROGRESS,
  7: STATUS.RESOLVED,
  8: STATUS.CLOSED,
};
