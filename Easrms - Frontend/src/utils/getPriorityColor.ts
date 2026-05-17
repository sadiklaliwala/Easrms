export const getPriorityColor = (
  priority: string
): "default" | "success" | "warning" | "error" => {
  const map: Record<string, "default" | "success" | "warning" | "error"> = {
    Low: "success",
    Medium: "warning",
    High: "error",
  };
  return map[priority] ?? "default";
};