// import { fetchBaseQuery } from '@reduxjs/toolkit/query/react'

// export const baseQuery = fetchBaseQuery({
//     baseUrl: 'https://localhost:7252/api',
//     credentials: 'include',
// })

import { fetchBaseQuery } from "@reduxjs/toolkit/query/react";

const baseQuery = fetchBaseQuery({
  baseUrl: import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7252",
  credentials: "include", // Required — JWT is in HttpOnly cookie
  prepareHeaders: (headers) => {
    headers.set("Content-Type", "application/json");
    return headers;
  },
});

export default baseQuery;
