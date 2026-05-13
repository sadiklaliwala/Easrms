// ============================================================
// CATEGORY TYPES
// ============================================================

// GET /api/categories - List Response
export interface CategoryListDto {
    categoryId: string
    categoryName: string
    isApprovalRequired: boolean
    isActive: boolean
    createdOn: string
}

// GET /api/categories/{id} - Detail Response
export interface CategoryDetailDto {
    categoryId: string
    categoryName: string
    isApprovalRequired: boolean
    isActive: boolean
    createdOn: string
    updatedOn: string | null
}

// POST /api/categories - Request Body
export interface CreateCategoryDto {
    categoryName: string
    isApprovalRequired: boolean
}

// PUT /api/categories/{id} - Request Body
export interface UpdateCategoryDto {
    categoryName: string
    isApprovalRequired: boolean
}
