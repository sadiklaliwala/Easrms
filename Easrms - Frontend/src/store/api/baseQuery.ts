// import { fetchBaseQuery } from '@reduxjs/toolkit/query/react'

// export const baseQuery = fetchBaseQuery({
//     baseUrl: 'https://localhost:7252/api',
//     credentials: 'include',
// })

import { fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type { BaseQueryFn } from "@reduxjs/toolkit/query";
import { clearCredentials } from "../slices/authSlice";
import type { ApiResponse } from "../../types/common.types";
import type { RefreshTokenResponseDto } from "../../types/auth.types";

const baseQuery = fetchBaseQuery({
  baseUrl: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5118",
  prepareHeaders: (headers) => {
    headers.set("Content-Type", "application/json");
    const accessToken = localStorage.getItem("accessToken");
    if (accessToken) {
      headers.set("Authorization", `Bearer ${accessToken}`);
    }
    return headers;
  },
});

export const baseQueryWithReauth: BaseQueryFn = async (
  args,
  api,
  extraOptions,
) => {
  let result = await baseQuery(args, api, extraOptions);

  if (result.error?.status === 401) {
    // Get the url from args — do not retry auth endpoints themselves
    const url = typeof args === "string" ? args : args?.url;
    const isAuthEndpoint =
      url?.includes("/api/Auth/me") ||
      url?.includes("/api/Auth/refresh-token") ||
      url?.includes("/api/Auth/login");

    if (!isAuthEndpoint) {
      const accessToken = localStorage.getItem("accessToken");
      const refreshToken = localStorage.getItem("refreshToken");

      if (refreshToken) {
        const refreshResult = await baseQuery(
          {
            url: "/api/Auth/refresh-token",
            method: "POST",
            body: { accessToken, refreshToken },
          },
          api,
          extraOptions,
        );

        const refreshData = refreshResult.data as
          | ApiResponse<RefreshTokenResponseDto>
          | undefined;

        if (refreshData && refreshData.success && refreshData.data) {
          localStorage.setItem("accessToken", refreshData.data.accessToken);
          localStorage.setItem("refreshToken", refreshData.data.refreshToken);

          // Retry the original query
          result = await baseQuery(args, api, extraOptions);
        } else {
          localStorage.removeItem("accessToken");
          localStorage.removeItem("refreshToken");
          api.dispatch(clearCredentials());
        }
      } else {
        localStorage.removeItem("accessToken");
        api.dispatch(clearCredentials());
      }
    }
  }

  return result;
};
export default baseQuery;
