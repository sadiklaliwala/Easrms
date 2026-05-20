import { api } from "./api";
import ApiEndPoints from "../ApiEndPoints";
import type {
  ApiResponse,
  SupportUserLookupDto,
  ManagerLookupDto,
} from "../../types/common.types";

const lookupEndpoints = api.injectEndpoints({
  endpoints: (builder) => ({
    getSupportUsers: builder.query<ApiResponse<SupportUserLookupDto[]>, void>({
      query: () => ApiEndPoints.LOOKUP.SUPPORT_USERS,
      providesTags: [{ type: "Lookup", id: "SUPPORT_USERS" }],
    }),

    getManagers: builder.query<ApiResponse<ManagerLookupDto[]>, void>({
      query: () => ApiEndPoints.LOOKUP.MANAGERS,
      providesTags: [{ type: "Lookup", id: "MANAGERS" }],
    }),
  }),
});

export const { useGetSupportUsersQuery, useGetManagersQuery } = lookupEndpoints;
