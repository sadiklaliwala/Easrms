import { api } from './Api'
import type { SupportUserLookupDto, ManagerLookupDto } from '../../types/common.types'
import ApiEndPoints from '../ApiEndPoints'

export const lookupEndpoints = api.injectEndpoints({
    endpoints: (builder) => ({

        getSupportUsers: builder.query<SupportUserLookupDto[], void>({
            query: () => ApiEndPoints.Lookup.SupportUsers,
            providesTags: ['Lookup'],
        }),

        getManagers: builder.query<ManagerLookupDto[], void>({
            query: () => ApiEndPoints.Lookup.Managers,
            providesTags: ['Lookup'],
        }),

    }),
    overrideExisting: false,
})

export const {
    useGetSupportUsersQuery,
    useGetManagersQuery,
} = lookupEndpoints