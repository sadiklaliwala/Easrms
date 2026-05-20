// import { api } from './Api'
// import type { DashboardSummaryDto } from '../../types/dashboard.types'
// import ApiEndPoints from '../ApiEndPoints'

// export const dashboardEndpoints = api.injectEndpoints({
//   endpoints: (builder) => ({

//     getDashboardSummary: builder.query<DashboardSummaryDto, void>({
//       query: () => ApiEndPoints.Dashboard.Summary,
//       providesTags: ['Dashboard'],
//     }),

//   }),
//   overrideExisting: false,
// })

// export const {
//   useGetDashboardSummaryQuery,
// } = dashboardEndpoints

import { api } from './api';
import ApiEndPoints from '../ApiEndPoints';
import type { ApiResponse } from '../../types/common.types';
import type { DashboardSummaryDto, SLADashboardDto } from '../../types/dashboard.types';

const dashboardEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getDashboardSummary: builder.query<ApiResponse<DashboardSummaryDto>, void>({
      query: () => ApiEndPoints.DASHBOARD.SUMMARY,
      providesTags: ['Dashboard'],
    }),
    getSLADashboard: builder.query<ApiResponse<SLADashboardDto>, void>({
      query: () => '/api/Dashboard/sla-summary',
      providesTags: ['Dashboard'],
    }),
  }),
});

export const { useGetDashboardSummaryQuery, useGetSLADashboardQuery } = dashboardEndpoints;