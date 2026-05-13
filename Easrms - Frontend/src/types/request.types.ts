import type { PaginationDto } from './common.types'

// ============================================================
// REQUEST TYPES
// ============================================================

// GET /api/requests - List Response
export interface RequestListDto {
    requestId: string
    requestNumber: string
    title: string
    categoryName: string
    priority: number
    status: number
    createdOn: string
    assigneeName: string
}

// GET /api/requests/{id} - Detail Response
export interface RequestDetailDto {
    requestId: string
    requestNumber: string
    title: string
    description: string
    categoryName: string
    priority: number
    status: number
    employeeName: string
    assigneeName: string
    createdOn: string
    updatedOn: string | null
    resolvedOn: string | null
    closedOn: string | null
    rejectionReason: string
}

// POST /api/requests - Request Body
export interface CreateRequestDto {
    categoryId: string
    title: string
    description: string
    priority: number
}

// POST /api/requests/{id}/approval - Request Body
export interface ApprovalRequestDto {
    action: string
    comment: string
}

// POST /api/requests/{id}/assign - Request Body
export interface AssignRequestDto {
    supportUserId: string
}

// POST /api/requests/{id}/status - Request Body
export interface UpdateStatusDto {
    newStatus: number
    remarks: string
}

// Paginated list response wrapper
export interface RequestListWithPaginationDto {
    items: RequestListDto[]
    pagination: PaginationDto
}