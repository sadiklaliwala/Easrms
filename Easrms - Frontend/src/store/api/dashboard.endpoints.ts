import { api } from './Api'
import type { DashboardSummaryDto } from '../../types/dashboard.types'
import ApiEndPoints from '../ApiEndPoints'

export const dashboardEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({

    getDashboardSummary: builder.query<DashboardSummaryDto, void>({
      query: () => ApiEndPoints.Dashboard.Summary,
      providesTags: ['Dashboard'],
    }),

  }),
  overrideExisting: false,
})

export const {
  useGetDashboardSummaryQuery,
} = dashboardEndpoints