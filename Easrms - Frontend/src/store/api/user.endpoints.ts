import { api } from './Api'
import type { UserListDto, UserDetailDto, CreateUserDto, UpdateUserDto } from '../../types/user.types'
import ApiEndPoints from '../ApiEndPoints';

export const userEndpoints = api.injectEndpoints({
    endpoints: (builder) => ({

        getUsers: builder.query<UserListDto[], void>({
            query: () => ApiEndPoints.Users.GetAll,
            providesTags: ['User'],
        }),

        getUserById: builder.query<UserDetailDto, string>({
            query: (id) => ApiEndPoints.Users.GetById(id),
            providesTags: ['User'],
        }),

        createUser: builder.mutation<void, CreateUserDto>({
            query: (body) => ({
                url: ApiEndPoints.Users.Create,
                method: 'POST',
                body,
            }),
            invalidatesTags: ['User'],
        }),

        updateUser: builder.mutation<void, { id: string; body: UpdateUserDto }>({
            query: ({ id, body }) => ({
                url: ApiEndPoints.Users.Update(id),
                method: 'PUT',
                body,
            }),
            invalidatesTags: ['User'],
        }),

        toggleUserStatus: builder.mutation<void, string>({
            query: (id) => ({
                url: ApiEndPoints.Users.ToggleStatus(id),
                method: 'PUT',
            }),
            invalidatesTags: ['User'],
        }),

    }),
    overrideExisting: false,
})

export const {
    useGetUsersQuery,
    useGetUserByIdQuery,
    useCreateUserMutation,
    useUpdateUserMutation,
    useToggleUserStatusMutation,
} = userEndpoints