// // ============================================================
// // USER TYPES
// // ============================================================

// // GET /api/users - List Response
// export interface UserListDto {
//     userId: string
//     fullName: string
//     email: string
//     roleName: string
//     isActive: boolean
//     createdOn: string
// }

// // GET /api/users/{id} - Detail Response
// export interface UserDetailDto {
//     userId: string
//     fullName: string
//     email: string
//     roleName: string
//     managerId: string | null
//     managerName: string
//     isActive: boolean
//     createdOn: string
//     updatedOn: string | null
//     lastLoginOn: string | null
// }

// // POST /api/users - Request Body
// export interface CreateUserDto {
//     fullName: string
//     email: string
//     password: string
//     roleId: string
//     managerId?: string
// }

// // PUT /api/users/{id} - Request Body
// export interface UpdateUserDto {
//     fullName: string
//     email: string
//     roleId: string
//     managerId?: string
// }

import { PaginationDto } from "./common.types";

export interface UserListDto {
  userId: string;
  fullName: string;
  email: string;
  roleName: string;
  isActive: boolean;
  createdOn: string;
}

export interface UserDetailDto {
  userId: string;
  fullName: string;
  email: string;
  roleName: string;
  managerId: string | null;
  managerName: string;
  isActive: boolean;
  createdOn: string;
  updatedOn: string | null;
  lastLoginOn: string | null;
}

export interface CreateUserDto {
  fullName: string;
  email: string;
  password: string;
  roleId: string;
  managerId: string | null;
}

export interface UpdateUserDto {
  fullName: string;
  email: string;
  roleId: string;
  managerId: string | null;
}

export interface UserListWithPaginationDto {
  items: UserListDto[];
  pagination: PaginationDto;
}

export interface UserQueryParams {
  pageNumber?: number;
  pageSize?: number;
  search?: string;
  roleId?: string;
  isActive?: boolean;
}

