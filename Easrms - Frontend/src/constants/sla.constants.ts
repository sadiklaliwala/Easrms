export const SLA_STATUS = {
  WITHIN: "Within SLA",
  NEARING: "Nearing Breach",
  BREACHED: "Breached",
  NA: "N/A",
} as const;

export const SLA_NEARING_BREACH_HOURS = 2;

export const SLA_STATUS_COLOR: Record<string, string> = {
  "Within SLA": "success",
  "Nearing Breach": "warning",
  "Breached": "error",
  "N/A": "default",
};
