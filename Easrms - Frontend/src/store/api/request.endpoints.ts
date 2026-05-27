// import { api } from './Api'
// import type { RequestDetailDto } from '../../types/request.types'
// import type { CreateRequestDto } from '../../types/request.types'
// import type { ApprovalRequestDto } from '../../types/request.types'
// import type { AssignRequestDto } from '../../types/request.types'
// import type { UpdateStatusDto } from '../../types/request.types'
// import type { RequestListWithPaginationDto } from '../../types/request.types'
// import ApiEndPoints from '../ApiEndPoints'

// export const requestEndpoints = api.injectEndpoints({
//     endpoints: (builder) => ({

//         getRequests: builder.query<RequestListWithPaginationDto, {
//             status?: string
//             priority?: string
//             categoryId?: string
//             search?: string
//             page?: number
//             pageSize?: number
//             assignedToMe?: boolean
//         }>({
//             query: (params) => ({
//                 url: ApiEndPoints.Requests.GetAll,
//                 params,
//             }),
//             providesTags: ['Request'],
//         }),

//         getRequestById: builder.query<RequestDetailDto, string>({
//             query: (id) => ApiEndPoints.Requests.GetById(id),
//             providesTags: ['Request'],
//         }),

//         createRequest: builder.mutation<void, CreateRequestDto>({
//             query: (body) => ({
//                 url: ApiEndPoints.Requests.Create,
//                 method: 'POST',
//                 body,
//             }),
//             invalidatesTags: ['Request'],
//         }),

//         approveRequest: builder.mutation<void, { id: string; body: ApprovalRequestDto }>({
//             query: ({ id, body }) => ({
//                 url: ApiEndPoints.Requests.Approve(id),
//                 method: 'POST',
//                 body,
//             }),
//             invalidatesTags: ['Request'],
//         }),

//         assignRequest: builder.mutation<void, { id: string; body: AssignRequestDto }>({
//             query: ({ id, body }) => ({
//                 url: ApiEndPoints.Requests.Assign(id),
//                 method: 'POST',
//                 body,
//             }),
//             invalidatesTags: ['Request'],
//         }),

//         updateRequestStatus: builder.mutation<void, { id: string; body: UpdateStatusDto }>({
//             query: ({ id, body }) => ({
//                 url: ApiEndPoints.Requests.UpdateStatus(id),
//                 method: 'POST',
//                 body,
//             }),
//             invalidatesTags: ['Request'],
//         }),

//         closeRequest: builder.mutation<void, string>({
//             query: (id) => ({
//                 url: ApiEndPoints.Requests.Close(id),
//                 method: 'PUT',
//             }),
//             invalidatesTags: ['Request'],
//         }),

//     }),
//     overrideExisting: false,
// })

// export const {
//     useGetRequestsQuery,
//     useGetRequestByIdQuery,
//     useCreateRequestMutation,
//     useApproveRequestMutation,
//     useAssignRequestMutation,
//     useUpdateRequestStatusMutation,
//     useCloseRequestMutation,
// } = requestEndpoints

import { api } from "./api";
import ApiEndPoints from "../ApiEndPoints";
import type { ApiResponse } from "../../types/common.types";
import type { BulkUploadResult } from "../../types/bulkUpload.types";
import type {
  RequestListWithPaginationDto,
  RequestDetailDto,
  CreateRequestDto,
  ApprovalRequestDto,
  AssignRequestDto,
  UpdateStatusDto,
  RequestQueryParams,
  EscalateRequestDto,
} from "../../types/request.types";
import { buildQueryParams } from "../../utils/buildQueryParams";

const requestEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getRequests: builder.query<
      ApiResponse<RequestListWithPaginationDto>,
      RequestQueryParams
    >({
      query: (params) =>
        `${ApiEndPoints.REQUESTS.BASE}${buildQueryParams(params)}`,
      providesTags: ["Request"],
    }),

    getRequestById: builder.query<ApiResponse<RequestDetailDto>, string>({
      query: (id) => ApiEndPoints.REQUESTS.BY_ID(id),
      providesTags: (_result, _error, id) => [{ type: "Request", id }],
    }),

    createRequest: builder.mutation<
      ApiResponse<RequestDetailDto>,
      CreateRequestDto
    >({
      query: (body) => ({
        url: ApiEndPoints.REQUESTS.BASE,
        method: "POST",
        body,
      }),
      invalidatesTags: ["Request", "Dashboard"],
    }),

    bulkUploadRequests: builder.mutation<ApiResponse<BulkUploadResult>, FormData>({
      query: (formData) => ({
        url: `${ApiEndPoints.REQUESTS.BASE}/bulk`,
        method: "POST",
        body: formData,
      }),
      invalidatesTags: ["Request"],
    }),

    approveOrRejectRequest: builder.mutation<
      ApiResponse<null>,
      { id: string; body: ApprovalRequestDto }
    >({
      query: ({ id, body }) => ({
        url: ApiEndPoints.REQUESTS.APPROVAL(id),
        method: "POST",
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: "Request", id },
        "Request",
        "Dashboard",
      ],
    }),

    assignRequest: builder.mutation<
      ApiResponse<null>,
      { id: string; body: AssignRequestDto }
    >({
      query: ({ id, body }) => ({
        url: ApiEndPoints.REQUESTS.ASSIGN(id),
        method: "POST",
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: "Request", id },
        "Request",
        "Dashboard",
      ],
    }),

    updateRequestStatus: builder.mutation<
      ApiResponse<null>,
      { id: string; body: UpdateStatusDto }
    >({
      query: ({ id, body }) => ({
        url: ApiEndPoints.REQUESTS.STATUS(id),
        method: "POST",
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: "Request", id },
        "Request",
        "Dashboard",
      ],
    }),

    closeRequest: builder.mutation<ApiResponse<null>, string>({
      query: (id) => ({
        url: ApiEndPoints.REQUESTS.CLOSE(id),
        method: "PUT",
      }),
      invalidatesTags: (_result, _error, id) => [
        { type: "Request", id },
        "Request",
        "Dashboard",
      ],
    }),

    escalateRequest: builder.mutation<
      ApiResponse<null>,
      { id: string; body: EscalateRequestDto }
    >({
      query: ({ id, body }) => ({
        url: `${ApiEndPoints.REQUESTS.BY_ID(id)}/escalate`,
        method: "POST",
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: "Request", id },
        "Request",
      ],
    }),

    exportRequestListExcel: builder.query<Blob, RequestQueryParams>({
      query: (params) => ({
        url: `${ApiEndPoints.EXPORT.REQUESTS_EXCEL}${buildQueryParams(params)}`,
        responseHandler: (response: any) => response.blob(),
      }),
    }),

    exportRequestListPdf: builder.query<Blob, RequestQueryParams>({
      query: (params) => ({
        url: `${ApiEndPoints.EXPORT.REQUESTS_PDF}${buildQueryParams(params)}`,
        responseHandler: (response: any) => response.blob(),
      }),
    }),

    exportRequestDetailExcel: builder.query<Blob, string>({
      query: (id) => ({
        url: ApiEndPoints.EXPORT.REQUEST_DETAIL_EXCEL(id),
        responseHandler: (response: any) => response.blob(),
      }),
    }),

    exportRequestDetailPdf: builder.query<Blob, string>({
      query: (id) => ({
        url: ApiEndPoints.EXPORT.REQUEST_DETAIL_PDF(id),
        responseHandler: (response: any) => response.blob(),
      }),
    }),
  }),
});

export const {
  useGetRequestsQuery,
  useGetRequestByIdQuery,
  useCreateRequestMutation,
  useApproveOrRejectRequestMutation,
  useAssignRequestMutation,
  useUpdateRequestStatusMutation,
  useCloseRequestMutation,
  useEscalateRequestMutation,
  useLazyExportRequestListExcelQuery,
  useLazyExportRequestListPdfQuery,
  useLazyExportRequestDetailExcelQuery,
  useLazyExportRequestDetailPdfQuery,
  useBulkUploadRequestsMutation,
} = requestEndpoints;
