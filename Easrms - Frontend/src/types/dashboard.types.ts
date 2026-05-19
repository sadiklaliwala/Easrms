export interface PriorityCountDto {
  priority: number;
  count: number;
}

export interface CategoryCountDto {
  categoryName: string;
  count: number;
}

export interface AssignedUserCountDto {
  userId: string;
  fullName: string;
  count: number;
}

export interface DashboardSummaryDto {
  totalRequests: number;
  openCount: number;
  pendingApprovalCount: number;
  approvedCount: number;
  rejectedCount: number;
  assignedCount: number;
  inProgressCount: number;
  resolvedCount: number;
  closedCount: number;
  byPriority: PriorityCountDto[];
  byCategory: CategoryCountDto[];
  byAssignedUser?: AssignedUserCountDto[];  // new
}