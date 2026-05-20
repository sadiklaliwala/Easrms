import { SLA_NEARING_BREACH_HOURS, SLA_STATUS } from "../constants/sla.constants";
import type { SLAStatus } from "../types/common.types";

export const getSLAStatus = (
  dueDate: string | null,
  status: string
): SLAStatus => {
  if (status === "Resolved" || status === "Closed") return SLA_STATUS.WITHIN;
  if (!dueDate) return SLA_STATUS.NA;

  const now = new Date();
  const due = new Date(dueDate);
  const nearingThreshold = new Date(due.getTime() - SLA_NEARING_BREACH_HOURS * 60 * 60 * 1000);

  if (now > due) return SLA_STATUS.BREACHED;
  if (now > nearingThreshold) return SLA_STATUS.NEARING;
  return SLA_STATUS.WITHIN;
};
