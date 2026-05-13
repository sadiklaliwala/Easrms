import { fetchBaseQuery } from '@reduxjs/toolkit/query/react'

export const baseQuery = fetchBaseQuery({
    baseUrl: 'https://localhost:7252/api',
    credentials: 'include',
})
