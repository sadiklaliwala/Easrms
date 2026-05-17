export const PRIORITY = {
  LOW: "Low",
  MEDIUM: "Medium",
  HIGH: "High",
} as const;

export type PriorityType = (typeof PRIORITY)[keyof typeof PRIORITY];

export const PRIORITY_OPTIONS = Object.values(PRIORITY).map((p) => ({
  label: p,
  value: p,
}));

export const PRIORITY_ENUM: Record<string, number> = {
  Low: 1,
  Medium: 2,
  High: 3,
};

export const PRIORITY_ENUM_REVERSE: Record<number, string> = {
  1: "Low",
  2: "Medium",
  3: "High",
};