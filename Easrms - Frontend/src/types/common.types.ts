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

// export enum CommentTypeEnum {
//   Approval = 1,
//   Feedback = 2,
//   Resolution = 3,
// }

export const CommentTypeEnum = {
  Approval: 1,
  Feedback: 2,
  Resolution: 3,
} as const;

export type CommentTypeEnum =
  (typeof CommentTypeEnum)[keyof typeof CommentTypeEnum];

export interface AddCommentDto {
  commentText: string;
  commentType: CommentTypeEnum;
}

export interface CommentListDto {
  commentId: string;
  commentText: string;
  commentType: string;
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

export const AuthProvider = {
  Local: 1,
  Google: 2,
  GitHub: 3,
  Azure: 4,
  LinkedIn: 5,
} as const;

export type AuthProvider = (typeof AuthProvider)[keyof typeof AuthProvider];
