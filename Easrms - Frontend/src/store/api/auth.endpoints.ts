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

import { api } from "./api";
import ApiEndPoints from "../ApiEndPoints";
import type { ApiResponse } from "../../types/common.types";
import type {
  LoginRequestDto,
  LoginResponseDto,
  CurrentUserDto,
  RefreshTokenRequestDto,
  RefreshTokenResponseDto,
  OAuthLoginDto,
  LinkedProviderDto,
  LinkProviderDto,
  UnlinkProviderDto,
} from "../../types/auth.types";

const authEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    login: builder.mutation<ApiResponse<LoginResponseDto>, LoginRequestDto>({
      query: (body) => ({
        url: ApiEndPoints.AUTH.LOGIN,
        method: "POST",
        body,
      }),
    }),

    logout: builder.mutation<ApiResponse<null>, void>({
      query: () => ({
        url: ApiEndPoints.AUTH.LOGOUT,
        method: "POST",
      }),
    }),

    getMe: builder.query<ApiResponse<CurrentUserDto>, void>({
      query: () => ApiEndPoints.AUTH.ME,
    }),

    refreshToken: builder.mutation<
      ApiResponse<RefreshTokenResponseDto>,
      RefreshTokenRequestDto
    >({
      query: (body) => ({
        url: ApiEndPoints.AUTH.REFRESH_TOKEN,
        method: "POST",
        body,
      }),
    }),

    revokeToken: builder.mutation<ApiResponse<null>, { refreshToken: string }>({
      query: (body) => ({
        url: ApiEndPoints.AUTH.REVOKE_TOKEN,
        method: "POST",
        body,
      }),
    }),

    oauthLogin: builder.mutation<ApiResponse<LoginResponseDto>, OAuthLoginDto>({
      query: (body) => ({
        url: ApiEndPoints.AUTH.OAUTH_LOGIN,
        method: "POST",
        body,
      }),
    }),

    linkProvider: builder.mutation<ApiResponse<null>, LinkProviderDto>({
      query: (body) => ({
        url: ApiEndPoints.AUTH.LINK_PROVIDER,
        method: "POST",
        body,
      }),
      invalidatesTags: ["LinkedProviders"],
    }),

    unlinkProvider: builder.mutation<ApiResponse<null>, UnlinkProviderDto>({
      query: (body) => ({
        url: ApiEndPoints.AUTH.UNLINK_PROVIDER,
        method: "DELETE",
        body,
      }),
      invalidatesTags: ["LinkedProviders"],
    }),

    getLinkedProviders: builder.query<ApiResponse<LinkedProviderDto[]>, void>({
      query: () => ApiEndPoints.AUTH.LINKED_PROVIDERS,
      providesTags: ["LinkedProviders"],
    }),
  }),
});

export const {
  useLoginMutation,
  useLogoutMutation,
  useGetMeQuery,
  useRefreshTokenMutation,
  useRevokeTokenMutation,
  useOauthLoginMutation,
  useLinkProviderMutation,
  useUnlinkProviderMutation,
  useGetLinkedProvidersQuery,
} = authEndpoints;
