// export const PRIORITY = {
//   LOW: 1,
//   MEDIUM: 2,
//   HIGH: 3,
// } as const;

// export type PriorityType = (typeof PRIORITY)[keyof typeof PRIORITY];
// export const PRIORITY_OPTIONS = [
//   { label: "Low", value: PRIORITY.LOW },
//   { label: "Medium", value: PRIORITY.MEDIUM },
//   { label: "High", value: PRIORITY.HIGH },
// ];

// export const PRIORITY_ENUM: Record<string, number> = {
//   Low: 1,
//   Medium: 2,
//   High: 3,
// };

// export const PRIORITY_ENUM_REVERSE: Record<number, string> = {
//   1: "Low",
//   2: "Medium",
//   3: "High",
// };

// API VALUES
export const PRIORITY = {
  LOW: 1,
  MEDIUM: 2,
  HIGH: 3,
} as const;

// UI LABELS
export const PRIORITY_LABEL: Record<number, string> = {
  1: "Low",
  2: "Medium",
  3: "High",
};

// OPTIONS
export const PRIORITY_OPTIONS = [
  { label: PRIORITY_LABEL[PRIORITY.LOW], value: PRIORITY.LOW },
  { label: PRIORITY_LABEL[PRIORITY.MEDIUM], value: PRIORITY.MEDIUM },
  { label: PRIORITY_LABEL[PRIORITY.HIGH], value: PRIORITY.HIGH },
];

export type PriorityType = (typeof PRIORITY)[keyof typeof PRIORITY];
