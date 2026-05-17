// // ============================================================
// // DASHBOARD TYPES
// // ============================================================

// // Nested inside DashboardSummaryDto
// export interface PriorityCountDto {
//     priority: number
//     count: number
// }

// // Nested inside DashboardSummaryDto
// export interface CategoryCountDto {
//     categoryName: string
//     count: number
// }

// // GET /api/dashboard/summary - Response
// export interface DashboardSummaryDto {
//     totalRequests: number
//     openCount: number
//     pendingApprovalCount: number
//     approvedCount: number
//     rejectedCount: number
//     assignedCount: number
//     inProgressCount: number
//     resolvedCount: number
//     closedCount: number
//     byPriority: PriorityCountDto[]
//     byCategory: CategoryCountDto[]
// }


export interface PriorityCountDto {
  priority: number;
  count: number;
}

export interface CategoryCountDto {
  categoryName: string;
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
}