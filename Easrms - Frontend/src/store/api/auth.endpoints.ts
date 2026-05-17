// import { api } from './Api'

// import type { LoginRequestDto, LoginResponseDto, CurrentUserDto } from '../../types/auth.types'
// import ApiEndPoints from '../ApiEndPoints'

// export const authEndpoints = api.injectEndpoints({
//     endpoints: (builder) => ({

//         login: builder.mutation<LoginResponseDto, LoginRequestDto>({
//             query: (body) => ({
//                 url: ApiEndPoints.Auth.Login,
//                 method: 'POST',
//                 body,
//             }),
//         }),

//         logout: builder.mutation<void, void>({
//             query: () => ({
//                 url: ApiEndPoints.Auth.Logout,
//                 method: 'POST',
//             }),
//         }),

//         getMe: builder.query<CurrentUserDto, void>({
//             query: () => ApiEndPoints.Auth.Me,
//         }),

//         refreshToken: builder.mutation<void, void>({
//             query: () => ({
//                 url: ApiEndPoints.Auth.RefreshToken,
//                 method: 'POST',
//             }),
//         }),

//         revokeToken: builder.mutation<void, void>({
//             query: () => ({
//                 url: ApiEndPoints.Auth.RevokeToken,
//                 method: 'POST',
//             }),
//         }),

//     }),
//     overrideExisting: false,
// })

// export const {
//     useLoginMutation,
//     useLogoutMutation,
//     useGetMeQuery,
//     useRefreshTokenMutation,
//     useRevokeTokenMutation,
// } = authEndpoints


import { api } from './api';
import ApiEndPoints from '../ApiEndPoints';
import type { ApiResponse } from '../../types/common.types';
import type {
  LoginRequestDto,
  LoginResponseDto,
  CurrentUserDto,
  RefreshTokenRequestDto,
  RefreshTokenResponseDto,
} from '../../types/auth.types';

const authEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<ApiResponse<LoginResponseDto>, LoginRequestDto>({
      query: (body) => ({
        url: ApiEndPoints.AUTH.LOGIN,
        method: 'POST',
        body,
      }),
    }),

    logout: builder.mutation<ApiResponse<null>, void>({
      query: () => ({
        url: ApiEndPoints.AUTH.LOGOUT,
        method: 'POST',
      }),
    }),

    getMe: builder.query<ApiResponse<CurrentUserDto>, void>({
      query: () => ApiEndPoints.AUTH.ME,
    }),

    refreshToken: builder.mutation<ApiResponse<RefreshTokenResponseDto>, RefreshTokenRequestDto>({
      query: (body) => ({
        url: ApiEndPoints.AUTH.REFRESH_TOKEN,
        method: 'POST',
        body,
      }),
    }),

    revokeToken: builder.mutation<ApiResponse<null>, { refreshToken: string }>({
      query: (body) => ({
        url: ApiEndPoints.AUTH.REVOKE_TOKEN,
        method: 'POST',
        body,
      }),
    }),
  }),
});

export const {
  useLoginMutation,
  useLogoutMutation,
  useGetMeQuery,
  useRefreshTokenMutation,
  useRevokeTokenMutation,
} = authEndpoints;