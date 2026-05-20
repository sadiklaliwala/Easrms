// import { fetchBaseQuery } from '@reduxjs/toolkit/query/react'

// export const baseQuery = fetchBaseQuery({
//     baseUrl: 'https://localhost:7252/api',
//     credentials: 'include',
// })

import { fetchBaseQuery } from "@reduxjs/toolkit/query/react";
import type { BaseQueryFn } from "@reduxjs/toolkit/query";
import { clearCredentials } from "../slices/authSlice";

const baseQuery = fetchBaseQuery({
  baseUrl: import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7252",
  credentials: "include", // Required — JWT is in HttpOnly cookie
  prepareHeaders: (headers) => {
    headers.set("Content-Type", "application/json");
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
      const refreshResult = await baseQuery(
        { url: "/api/Auth/refresh-token", method: "POST", body: {} },
        api,
        extraOptions,
      );

      if (refreshResult.data) {
        result = await baseQuery(args, api, extraOptions);
      } else {
        api.dispatch(clearCredentials());
      }
    }
  }

  return result;
};
export default baseQuery;
