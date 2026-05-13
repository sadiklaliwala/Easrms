import { api } from './Api'
import type { CommentListDto, AddCommentDto, StatusHistoryDto } from '../../types/comment.types'
import ApiEndPoints from '../ApiEndPoints';

export const commentEndpoints = api.injectEndpoints({
    endpoints: (builder) => ({

        getComments: builder.query<CommentListDto[], string>({
            query: (requestId) => ApiEndPoints.Comments.GetAll(requestId),
            providesTags: ['Comment'],
        }),

        addComment: builder.mutation<void, { requestId: string; body: AddCommentDto }>({
            query: ({ requestId, body }) => ({
                url: ApiEndPoints.Comments.Add(requestId),
                method: 'POST',
                body,
            }),
            invalidatesTags: ['Comment'],
        }),

        getHistory: builder.query<StatusHistoryDto[], string>({
            query: (requestId) => ApiEndPoints.History.GetAll(requestId),
            providesTags: ['History'],
        }),

    }),
    overrideExisting: false,
})

export const {
    useGetCommentsQuery,
    useAddCommentMutation,
    useGetHistoryQuery,
} = commentEndpoints