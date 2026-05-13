import { api } from './Api'
import type { CategoryListDto, CategoryDetailDto, CreateCategoryDto, UpdateCategoryDto } from '../../types/category.types'
import ApiEndPoints from '../ApiEndPoints';

export const categoryEndpoints = api.injectEndpoints({
    endpoints: (builder) => ({

        getCategories: builder.query<CategoryListDto[], void>({
            query: () => ApiEndPoints.Categories.GetAll,
            providesTags: ['Category'],
        }),

        getCategoryById: builder.query<CategoryDetailDto, string>({
            query: (id) => ApiEndPoints.Categories.GetById(id),
            providesTags: ['Category'],
        }),

        createCategory: builder.mutation<void, CreateCategoryDto>({
            query: (body) => ({
                url: ApiEndPoints.Categories.Create,
                method: 'POST',
                body,
            }),
            invalidatesTags: ['Category'],
        }),

        updateCategory: builder.mutation<void, { id: string; body: UpdateCategoryDto }>({
            query: ({ id, body }) => ({
                url: ApiEndPoints.Categories.Update(id),
                method: 'PUT',
                body,
            }),
            invalidatesTags: ['Category'],
        }),

        toggleCategoryStatus: builder.mutation<void, string>({
            query: (id) => ({
                url: ApiEndPoints.Categories.ToggleStatus(id),
                method: 'PUT',
            }),
            invalidatesTags: ['Category'],
        }),

    }),
    overrideExisting: false,
})

export const {
    useGetCategoriesQuery,
    useGetCategoryByIdQuery,
    useCreateCategoryMutation,
    useUpdateCategoryMutation,
    useToggleCategoryStatusMutation,
} = categoryEndpoints;