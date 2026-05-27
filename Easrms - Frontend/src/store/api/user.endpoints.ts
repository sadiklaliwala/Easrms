import { api } from "./api";
import ApiEndPoints from "../ApiEndPoints";
import type { ApiResponse } from "../../types/common.types";
import type {
  UserListWithPaginationDto,
  UserDetailDto,
  CreateUserDto,
  UpdateUserDto,
  UserQueryParams,
} from "../../types/user.types";
import { buildQueryParams } from "../../utils/buildQueryParams";
import type { BulkUploadResult } from "../../types/bulkUpload.types";

const userEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getUsers: builder.query<
      ApiResponse<UserListWithPaginationDto>,
      UserQueryParams
    >({
      query: (params) =>
        `${ApiEndPoints.USERS.BASE}${buildQueryParams(params)}`,
      providesTags: ["User"],
    }),

    getUserById: builder.query<ApiResponse<UserDetailDto>, string>({
      query: (id) => ApiEndPoints.USERS.BY_ID(id),
      providesTags: (_result, _error, id) => [{ type: "User", id }],
    }),

    createUser: builder.mutation<ApiResponse<UserDetailDto>, CreateUserDto>({
      query: (body) => ({
        url: ApiEndPoints.USERS.BASE,
        method: "POST",
        body,
      }),
      invalidatesTags: ["User", "Lookup"],
    }),

    bulkUploadUsers: builder.mutation<ApiResponse<BulkUploadResult>, FormData>({
      query: (formData) => ({
        url: `${ApiEndPoints.USERS.BASE}/bulk`,
        method: "POST",
        body: formData,
      }),
      invalidatesTags: ["User"],
    }),

    updateUser: builder.mutation<
      ApiResponse<UserDetailDto>,
      { id: string; body: UpdateUserDto }
    >({
      query: ({ id, body }) => ({
        url: ApiEndPoints.USERS.BY_ID(id),
        method: "PUT",
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: "User", id },
        "User",
        "Lookup",
      ],
    }),

    toggleUserStatus: builder.mutation<ApiResponse<null>, string>({
      query: (id) => ({
        url: ApiEndPoints.USERS.TOGGLE(id),
        method: "PUT",
      }),
      invalidatesTags: ["User", "Lookup"],
    }),
  }),
});

export const {
  useGetUsersQuery,
  useGetUserByIdQuery,
  useCreateUserMutation,
  useUpdateUserMutation,
  useToggleUserStatusMutation,
  useBulkUploadUsersMutation,
} = userEndpoints;
