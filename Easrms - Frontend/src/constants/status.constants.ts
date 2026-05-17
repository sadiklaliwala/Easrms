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
  "Pending Approval": 2,
  Approved: 3,
  Rejected: 4,
  Assigned: 5,
  "In Progress": 6,
  Resolved: 7,
  Closed: 8,
};

export const STATUS_ENUM_REVERSE: Record<number, string> = {
  1: "Open",
  2: "Pending Approval",
  3: "Approved",
  4: "Rejected",
  5: "Assigned",
  6: "In Progress",
  7: "Resolved",
  8: "Closed",
};