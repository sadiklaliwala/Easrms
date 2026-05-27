// import { api } from './Api'
// import type { CategoryListDto, CategoryDetailDto, CreateCategoryDto, UpdateCategoryDto } from '../../types/category.types'
// import ApiEndPoints from '../ApiEndPoints';

// export const categoryEndpoints = api.injectEndpoints({
//     endpoints: (builder) => ({

//         getCategories: builder.query<CategoryListDto[], void>({
//             query: () => ApiEndPoints.Categories.GetAll,
//             providesTags: ['Category'],
//         }),

//         getCategoryById: builder.query<CategoryDetailDto, string>({
//             query: (id) => ApiEndPoints.Categories.GetById(id),
//             providesTags: ['Category'],
//         }),

//         createCategory: builder.mutation<void, CreateCategoryDto>({
//             query: (body) => ({
//                 url: ApiEndPoints.Categories.Create,
//                 method: 'POST',
//                 body,
//             }),
//             invalidatesTags: ['Category'],
//         }),

//         updateCategory: builder.mutation<void, { id: string; body: UpdateCategoryDto }>({
//             query: ({ id, body }) => ({
//                 url: ApiEndPoints.Categories.Update(id),
//                 method: 'PUT',
//                 body,
//             }),
//             invalidatesTags: ['Category'],
//         }),

//         toggleCategoryStatus: builder.mutation<void, string>({
//             query: (id) => ({
//                 url: ApiEndPoints.Categories.ToggleStatus(id),
//                 method: 'PUT',
//             }),
//             invalidatesTags: ['Category'],
//         }),

//     }),
//     overrideExisting: false,
// })

// export const {
//     useGetCategoriesQuery,
//     useGetCategoryByIdQuery,
//     useCreateCategoryMutation,
//     useUpdateCategoryMutation,
//     useToggleCategoryStatusMutation,
// } = categoryEndpoints;

import { api } from "./api";
import ApiEndPoints from "../ApiEndPoints";
import type { ApiResponse } from "../../types/common.types";
import type { BulkUploadResult } from "../../types/bulkUpload.types";
import type {
  CategoryListWithPaginationDto,
  CategoryDetailDto,
  CreateCategoryDto,
  UpdateCategoryDto,
  CategoryQueryParams,
} from "../../types/category.types";
import { buildQueryParams } from "../../utils/buildQueryParams";

const categoryEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getCategories: builder.query<
      ApiResponse<CategoryListWithPaginationDto>,
      CategoryQueryParams
    >({
      query: (params) =>
        `${ApiEndPoints.CATEGORIES.BASE}${buildQueryParams(params)}`,
      providesTags: ["Category"],
    }),

    getCategoryById: builder.query<ApiResponse<CategoryDetailDto>, string>({
      query: (id) => ApiEndPoints.CATEGORIES.BY_ID(id),
      providesTags: (_result, _error, id) => [{ type: "Category", id }],
    }),

    createCategory: builder.mutation<
      ApiResponse<CategoryDetailDto>,
      CreateCategoryDto
    >({
      query: (body) => ({
        url: ApiEndPoints.CATEGORIES.BASE,
        method: "POST",
        body,
      }),
      invalidatesTags: ["Category"],
    }),

    bulkUploadCategories: builder.mutation<ApiResponse<BulkUploadResult>, FormData>({
      query: (formData) => ({
        url: `${ApiEndPoints.CATEGORIES.BASE}/bulk`,
        method: "POST",
        body: formData,
      }),
      invalidatesTags: ["Category"],
    }),

    updateCategory: builder.mutation<
      ApiResponse<CategoryDetailDto>,
      { id: string; body: UpdateCategoryDto }
    >({
      query: ({ id, body }) => ({
        url: ApiEndPoints.CATEGORIES.BY_ID(id),
        method: "PUT",
        body,
      }),
      invalidatesTags: (_result, _error, { id }) => [
        { type: "Category", id },
        "Category",
      ],
    }),

    toggleCategoryStatus: builder.mutation<ApiResponse<null>, string>({
      query: (id) => ({
        url: ApiEndPoints.CATEGORIES.TOGGLE(id),
        method: "PUT",
      }),
      invalidatesTags: ["Category"],
    }),
  }),
});

export const {
  useGetCategoriesQuery,
  useGetCategoryByIdQuery,
  useCreateCategoryMutation,
  useUpdateCategoryMutation,
  useToggleCategoryStatusMutation,
  useBulkUploadCategoriesMutation,
} = categoryEndpoints;
