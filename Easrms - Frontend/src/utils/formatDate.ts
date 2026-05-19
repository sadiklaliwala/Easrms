import { format, parseISO, isValid } from "date-fns";

export const formatDate = (
  dateStr: string | null | undefined,
  pattern: string = "dd MMM yyyy, hh:mm a"
): string => {
  if (!dateStr) return "—";
  try {
    const date = parseISO(dateStr);
    if (!isValid(date)) return "—";
    return format(date, pattern);
  } catch {
    return "—";
  }
};

export const formatDateOnly = (dateStr: string | null | undefined): string => {
  return formatDate(dateStr, "dd MMM yyyy");
};