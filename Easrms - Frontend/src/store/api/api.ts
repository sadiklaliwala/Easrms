// import { createApi, fetchBaseQuery, type BaseQueryFn } from '@reduxjs/toolkit/query/react'
// import { clearCredentials } from '../slices/authSlice'

// // ============================================================
// // BASE QUERY WITH REAUTH
// // ============================================================

// const baseQuery = fetchBaseQuery({
//     baseUrl: 'https://localhost:7000/api',
//     credentials: 'include',
// })

// const baseQueryWithReauth: BaseQueryFn = async (args, api, extraOptions) => {
//     let result = await baseQuery(args, api, extraOptions)
//     if (result.error && result.error.status === 401) {
//         const refreshResult = await baseQuery(
//             { url: '/auth/refresh-token', method: 'POST' },
//             api,
//             extraOptions
//         )
//         if (refreshResult.data) {
//             result = await baseQuery(args, api, extraOptions)
//         } else {
//             api.dispatch(clearCredentials())
//         }
//     }
//     return result
// }

// // ============================================================
// // SINGLE API SLICE
// // ============================================================

// export const api = createApi({
//     reducerPath: 'api',
//     baseQuery: baseQueryWithReauth,
//     tagTypes: ['Auth', 'User', 'Category', 'Request', 'Comment', 'History', 'Dashboard', 'Lookup'],
//     endpoints: () => ({}),
// })


import { createApi } from '@reduxjs/toolkit/query/react';
import baseQuery from './baseQuery';

export const api = createApi({
  reducerPath: 'api',
  baseQuery,
  tagTypes: ['User', 'Category', 'Request', 'Comment', 'History', 'Dashboard'],
  endpoints: () => ({}),
});