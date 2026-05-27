// import type { PaginationDto } from './common.types'

// // ============================================================
// // REQUEST TYPES
// // ============================================================

// // GET /api/requests - List Response
// export interface RequestListDto {
//     requestId: string
//     requestNumber: string
//     title: string
//     categoryName: string
//     priority: number
//     status: number
//     createdOn: string
//     assigneeName: string
// }

// // GET /api/requests/{id} - Detail Response
// export interface RequestDetailDto {
//     requestId: string
//     requestNumber: string
//     title: string
//     description: string
//     categoryName: string
//     priority: number
//     status: number
//     employeeName: string
//     assigneeName: string
//     createdOn: string
//     updatedOn: string | null
//     resolvedOn: string | null
//     closedOn: string | null
//     rejectionReason: string
// }

// // POST /api/requests - Request Body
// export interface CreateRequestDto {
//     categoryId: string
//     title: string
//     description: string
//     priority: number
// }

// // POST /api/requests/{id}/approval - Request Body
// export interface ApprovalRequestDto {
//     action: string
//     comment: string
// }

// // POST /api/requests/{id}/assign - Request Body
// export interface AssignRequestDto {
//     supportUserId: string
// }

// // POST /api/requests/{id}/status - Request Body
// export interface UpdateStatusDto {
//     newStatus: number
//     remarks: string
// }

// // Paginated list response wrapper
// export interface RequestListWithPaginationDto {
//     items: RequestListDto[]
//     pagination: PaginationDto
// }

import type { PriorityType } from "../constants/priority.constants";
import { type PaginationDto } from "./common.types";

export interface CreateRequestDto {
  categoryId: string;
  title: string;
  description: string;
  priority: PriorityType;
  attachmentUrl?: string | null;
}

export interface RequestListDto {
  requestId: string;
  requestNumber: string;
  title: string;
  categoryName: string;
  priority: number;
  status: number;
  createdOn: string;
  assigneeName: string;
  dueDate: string | null;
  slaStatus: string;
  isEscalated: boolean;
}

export interface RequestDetailDto {
  requestId: string;
  requestNumber: string;
  title: string;
  description: string;
  categoryName: string;
  priority: number;
  status: number;
  employeeName: string;
  assigneeName: string;
  createdOn: string;
  updatedOn: string | null;
  resolvedOn: string | null;
  closedOn: string | null;
  rejectionReason: string;
  dueDate: string | null;
  slaStatus: string;
  isEscalated: boolean;
  escalatedOn: string | null;
  escalatedByName: string | null;
  escalationReason: string | null;
  employeeId: string;
  assignedTo: string | null;
  attachmentUrl?: string | null;
}

export interface ApprovalRequestDto {
  action: string;
  comment: string;
}

export interface AssignRequestDto {
  supportUserId: string;
}

export interface UpdateStatusDto {
  newStatus: number;
  remarks: string;
}

export interface RequestListWithPaginationDto {
  items: RequestListDto[];
  pagination: PaginationDto;
}

export interface RequestQueryParams {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  priority?: number;
  categoryId?: string;
  fromDate?: string;
  toDate?: string;
  sortBy?: string;
  sortAscending?: boolean;
}

export type EscalateRequestDto = {
  escalationReason: string;
};

export interface ReopenRequestDto {
  reopenReason: string;
}
