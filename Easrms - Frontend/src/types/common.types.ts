// // ============================================================
// // COMMON TYPES
// // ============================================================

import type { ReactNode } from "react";

// // Generic wrapper for all API responses
// export interface ApiResponse<T = unknown> {
//     success: boolean
//     statusCode: number
//     message: string
//     data: T
//     errors: string[]
// }

// // Nested inside all paginated list responses
// export interface PaginationDto {
//     pageNumber: number
//     pageSize: number
//     totalCount: number
//     totalPages: number
// }

// // GET /api/lookup/support-users - Dropdown Response
// export interface SupportUserLookupDto {
//     userId: string
//     fullName: string
// }

// // GET /api/lookup/managers - Dropdown Response
// export interface ManagerLookupDto {
//     userId: string
//     fullName: string
// }

export interface PaginationDto {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data: T;
  errors: string[] | null;
}

export interface SupportUserLookupDto {
  userId: string;
  fullName: string;
}

export interface ManagerLookupDto {
  userId: string;
  fullName: string;
}
export interface GridColumn<T> {
  key: keyof T | "actions";
  label: string;
  render?: (row: T) => ReactNode;
  sortable?: boolean;
  width?: string | number;
}


export interface AddCommentDto {
  commentText: string;
  commentType: string;
}

export interface CommentListDto {
  commentId: string;
  commentText: string;
  commentType: number;
  commentByName: string;
  createdOn: string;
}

export interface StatusHistoryDto {
  historyId: string;
  oldStatus: number | null;
  newStatus: number;
  changedByName: string;
  changedOn: string;
  remarks: string;
}

export type SLAStatus = "Within SLA" | "Nearing Breach" | "Breached" | "N/A";
