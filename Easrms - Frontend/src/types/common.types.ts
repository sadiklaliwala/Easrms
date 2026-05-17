// // ============================================================
// // COMMON TYPES
// // ============================================================

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