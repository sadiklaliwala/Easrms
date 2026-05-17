// import { api } from './Api'

// import type { UserListDto, UserDetailDto, CreateUserDto, UpdateUserDto } from '../../types/user.types'
// import ApiEndPoints from '../ApiEndPoints';

// export const userEndpoints = api.injectEndpoints({
//     endpoints: (builder) => ({

//         getUsers: builder.query<UserListDto[], void>({
//             query: () => ApiEndPoints.Users.GetAll,
//             providesTags: ['User'],
//         }),

//         getUserById: builder.query<UserDetailDto, string>({
//             query: (id) => ApiEndPoints.Users.GetById(id),
//             providesTags: ['User'],
//         }),

//         createUser: builder.mutation<void, CreateUserDto>({
//             query: (body) => ({
//                 url: ApiEndPoints.Users.Create,
//                 method: 'POST',
//                 body,
//             }),
//             invalidatesTags: ['User'],
//         }),

//         updateUser: builder.mutation<void, { id: string; body: UpdateUserDto }>({
//             query: ({ id, body }) => ({
//                 url: ApiEndPoints.Users.Update(id),
//                 method: 'PUT',
//                 body,
//             }),
//             invalidatesTags: ['User'],
//         }),

//         toggleUserStatus: builder.mutation<void, string>({
//             query: (id) => ({
//                 url: ApiEndPoints.Users.ToggleStatus(id),
//                 method: 'PUT',
//             }),
//             invalidatesTags: ['User'],
//         }),

//     }),
//     overrideExisting: false,
// })

// export const {
//     useGetUsersQuery,
//     useGetUserByIdQuery,
//     useCreateUserMutation,
//     useUpdateUserMutation,
//     useToggleUserStatusMutation,
// } = userEndpoints

import { api } from './api';
import ApiEndPoints from '../ApiEndPoints';
import type { ApiResponse } from '../../types/common.types';
import type {
  UserListWithPaginationDto,
  UserDetailDto,
  CreateUserDto,
  UpdateUserDto,
  UserQueryParams,
} from '../../types/user.types';
import { buildQueryParams } from '../../utils/buildQueryParams';

const userEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getUsers: builder.query<ApiResponse<UserListWithPaginationDto>, UserQueryParams>({
      query: (params) => `${ApiEndPoints.USERS.BASE}?${buildQueryParams(params)}`,
      providesTags: ['User'],
    }),

    getUserById: builder.query<ApiResponse<UserDetailDto>, string>({
      query: (id) => ApiEndPoints.USERS.BY_ID(id),
      providesTags: (_result, _error, id) => [{ type: 'User', id }],
    }),

    createUser: builder.mutation<ApiResponse<UserDetailDto>, CreateUserDto>({
      query: (body) => ({
        url: ApiEndPoints.USERS.BASE,
        method: 'POST',
        body,
      }),
      invalidatesTags: ['User'],
    }),

    updateUser: builder.mutation<ApiResponse<UserDetailDto>, { id: string; body: UpdateUserDto }>({
      query: ({ id, body }) => ({
        url: ApiEndPoints.USERS.BY_ID(id),
        method: 'PUT',
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [{ type: 'User', id }, 'User'],
    }),

    toggleUserStatus: builder.mutation<ApiResponse<null>, string>({
      query: (id) => ({
        url: ApiEndPoints.USERS.TOGGLE(id),
        method: 'PUT',
      }),
      invalidatesTags: ['User'],
    }),
  }),
});

export const {
  useGetUsersQuery,
  useGetUserByIdQuery,
  useCreateUserMutation,
  useUpdateUserMutation,
  useToggleUserStatusMutation,
} = userEndpoints;