import { api } from './Api'
import type { RequestDetailDto } from '../../types/request.types'
import type { CreateRequestDto } from '../../types/request.types'
import type { ApprovalRequestDto } from '../../types/request.types'
import type { AssignRequestDto } from '../../types/request.types'
import type { UpdateStatusDto } from '../../types/request.types'
import type { RequestListWithPaginationDto } from '../../types/request.types'
import ApiEndPoints from '../ApiEndPoints'

export const requestEndpoints = api.injectEndpoints({
    endpoints: (builder) => ({

        getRequests: builder.query<RequestListWithPaginationDto, {
            status?: string
            priority?: string
            categoryId?: string
            search?: string
            page?: number
            pageSize?: number
            assignedToMe?: boolean
        }>({
            query: (params) => ({
                url: ApiEndPoints.Requests.GetAll,
                params,
            }),
            providesTags: ['Request'],
        }),

        getRequestById: builder.query<RequestDetailDto, string>({
            query: (id) => ApiEndPoints.Requests.GetById(id),
            providesTags: ['Request'],
        }),

        createRequest: builder.mutation<void, CreateRequestDto>({
            query: (body) => ({
                url: ApiEndPoints.Requests.Create,
                method: 'POST',
                body,
            }),
            invalidatesTags: ['Request'],
        }),

        approveRequest: builder.mutation<void, { id: string; body: ApprovalRequestDto }>({
            query: ({ id, body }) => ({
                url: ApiEndPoints.Requests.Approve(id),
                method: 'POST',
                body,
            }),
            invalidatesTags: ['Request'],
        }),

        assignRequest: builder.mutation<void, { id: string; body: AssignRequestDto }>({
            query: ({ id, body }) => ({
                url: ApiEndPoints.Requests.Assign(id),
                method: 'POST',
                body,
            }),
            invalidatesTags: ['Request'],
        }),

        updateRequestStatus: builder.mutation<void, { id: string; body: UpdateStatusDto }>({
            query: ({ id, body }) => ({
                url: ApiEndPoints.Requests.UpdateStatus(id),
                method: 'POST',
                body,
            }),
            invalidatesTags: ['Request'],
        }),

        closeRequest: builder.mutation<void, string>({
            query: (id) => ({
                url: ApiEndPoints.Requests.Close(id),
                method: 'PUT',
            }),
            invalidatesTags: ['Request'],
        }),

    }),
    overrideExisting: false,
})

export const {
    useGetRequestsQuery,
    useGetRequestByIdQuery,
    useCreateRequestMutation,
    useApproveRequestMutation,
    useAssignRequestMutation,
    useUpdateRequestStatusMutation,
    useCloseRequestMutation,
} = requestEndpoints