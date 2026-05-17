export const formatRequestNumber = (requestNumber: string): string => {
  if (!requestNumber) return "—";
  return requestNumber.toUpperCase();
};