// 

// import type { CommentListDto, AddCommentDto, StatusHistoryDto } from '../../types/comment.types'
// import ApiEndPoints from '../ApiEndPoints';

// export const commentEndpoints = api.injectEndpoints({
//     endpoints: (builder) => ({

//         getComments: builder.query<CommentListDto[], string>({
//             query: (requestId) => ApiEndPoints.Comments.GetAll(requestId),
//             providesTags: ['Comment'],
//         }),

//         addComment: builder.mutation<void, { requestId: string; body: AddCommentDto }>({
//             query: ({ requestId, body }) => ({
//                 url: ApiEndPoints.Comments.Add(requestId),
//                 method: 'POST',
//                 body,
//             }),
//             invalidatesTags: ['Comment'],
//         }),

//         getHistory: builder.query<StatusHistoryDto[], string>({
//             query: (requestId) => ApiEndPoints.History.GetAll(requestId),
//             providesTags: ['History'],
//         }),

//     }),
//     overrideExisting: false,
// })

// export const {
//     useGetCommentsQuery,
//     useAddCommentMutation,
//     useGetHistoryQuery,
// } = commentEndpoints

import { api } from './api';
import ApiEndPoints from '../ApiEndPoints';
import type { AddCommentDto, ApiResponse, CommentListDto, StatusHistoryDto } from '../../types/common.types';

const commentEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getComments: builder.query<ApiResponse<CommentListDto[]>, string>({
      query: (requestId) => ApiEndPoints.COMMENTS.BASE(requestId),
      providesTags: (_result, _error, requestId) => [{ type: 'Comment', id: requestId }],
    }),

    addComment: builder.mutation<ApiResponse<null>, { requestId: string; body: AddCommentDto }>({
      query: ({ requestId, body }) => ({
        url: ApiEndPoints.COMMENTS.BASE(requestId),
        method: 'POST',
        body,
      }),
      invalidatesTags: (_result, _error, { requestId }) => [{ type: 'Comment', id: requestId }],
    }),

    getStatusHistory: builder.query<ApiResponse<StatusHistoryDto[]>, string>({
      query: (requestId) => ApiEndPoints.COMMENTS.HISTORY(requestId),
      providesTags: (_result, _error, requestId) => [{ type: 'History', id: requestId }],
    }),
  }),
});

export const {
  useGetCommentsQuery,
  useAddCommentMutation,
  useGetStatusHistoryQuery,
} = commentEndpoints;