import { api } from './Api'
import type { LoginRequestDto, LoginResponseDto, CurrentUserDto } from '../../types/auth.types'
import ApiEndPoints from '../ApiEndPoints'

export const authEndpoints = api.injectEndpoints({
    endpoints: (builder) => ({

        login: builder.mutation<LoginResponseDto, LoginRequestDto>({
            query: (body) => ({
                url: ApiEndPoints.Auth.Login,
                method: 'POST',
                body,
            }),
        }),

        logout: builder.mutation<void, void>({
            query: () => ({
                url: ApiEndPoints.Auth.Logout,
                method: 'POST',
            }),
        }),

        getMe: builder.query<CurrentUserDto, void>({
            query: () => ApiEndPoints.Auth.Me,
        }),

        refreshToken: builder.mutation<void, void>({
            query: () => ({
                url: ApiEndPoints.Auth.RefreshToken,
                method: 'POST',
            }),
        }),

        revokeToken: builder.mutation<void, void>({
            query: () => ({
                url: ApiEndPoints.Auth.RevokeToken,
                method: 'POST',
            }),
        }),

    }),
    overrideExisting: false,
})

export const {
    useLoginMutation,
    useLogoutMutation,
    useGetMeQuery,
    useRefreshTokenMutation,
    useRevokeTokenMutation,
} = authEndpoints