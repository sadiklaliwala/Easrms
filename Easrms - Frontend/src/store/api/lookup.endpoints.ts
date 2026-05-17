// import { api } from './Api'


// import type { SupportUserLookupDto, ManagerLookupDto } from '../../types/common.types'
// import ApiEndPoints from '../ApiEndPoints'

// export const lookupEndpoints = api.injectEndpoints({
//     endpoints: (builder) => ({

//         getSupportUsers: builder.query<SupportUserLookupDto[], void>({
//             query: () => ApiEndPoints.Lookup.SupportUsers,
//             providesTags: ['Lookup'],
//         }),

//         getManagers: builder.query<ManagerLookupDto[], void>({
//             query: () => ApiEndPoints.Lookup.Managers,
//             providesTags: ['Lookup'],
//         }),

//     }),
//     overrideExisting: false,
// })

// export const {
//     useGetSupportUsersQuery,
//     useGetManagersQuery,
// } = lookupEndpoints

import { api } from './api';
import ApiEndPoints from '../ApiEndPoints';
import type { ApiResponse, SupportUserLookupDto, ManagerLookupDto } from '../../types/common.types';

const lookupEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getSupportUsers: builder.query<ApiResponse<SupportUserLookupDto[]>, void>({
      query: () => ApiEndPoints.LOOKUP.SUPPORT_USERS,
    }),

    getManagers: builder.query<ApiResponse<ManagerLookupDto[]>, void>({
      query: () => ApiEndPoints.LOOKUP.MANAGERS,
    }),
  }),
});

export const {
  useGetSupportUsersQuery,
  useGetManagersQuery,
} = lookupEndpoints;