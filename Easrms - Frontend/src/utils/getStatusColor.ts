export const getStatusColor = (
  status: string
): "default" | "warning" | "info" | "success" | "error" | "primary" | "secondary" => {
  const map: Record<string, "default" | "warning" | "info" | "success" | "error" | "primary" | "secondary"> = {
    Open: "info",
    "Pending Approval": "warning",
    Approved: "primary",
    Rejected: "error",
    Assigned: "secondary",
    "In Progress": "warning",
    Resolved: "success",
    Closed: "default",
  };
  return map[status] ?? "default";
};