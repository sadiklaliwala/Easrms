const ApiEndPoints = {
    baseUrl: 'https://localhost:7252/api',

    Auth: {
        Login: '/auth/login',
        Logout: '/auth/logout',
        Me: '/auth/me',
        RefreshToken: '/auth/refresh-token',
        RevokeToken: '/auth/revoke-token',
    },

    Users: {
        GetAll: '/users',
        GetById: (id: string) => `/users/${id}`,
        Create: '/users',
        Update: (id: string) => `/users/${id}`,
        ToggleStatus: (id: string) => `/users/${id}/activate-deactivate`,
    },

    Categories: {
        GetAll: '/categories',
        GetById: (id: string) => `/categories/${id}`,
        Create: '/categories',
        Update: (id: string) => `/categories/${id}`,
        ToggleStatus: (id: string) => `/categories/${id}/activate-deactivate`,
    },

    Requests: {
        GetAll: '/requests',
        GetById: (id: string) => `/requests/${id}`,
        Create: '/requests',
        Approve: (id: string) => `/requests/${id}/approval`,
        Assign: (id: string) => `/requests/${id}/assign`,
        UpdateStatus: (id: string) => `/requests/${id}/status`,
        Close: (id: string) => `/requests/${id}/close`,
    },

    Comments: {
        GetAll: (requestId: string) => `/requests/${requestId}/comments`,
        Add: (requestId: string) => `/requests/${requestId}/comments`,
    },

    History: {
        GetAll: (requestId: string) => `/requests/${requestId}/history`,
    },

    Dashboard: {
        Summary: '/dashboard/summary',
    },

    Lookup: {
        SupportUsers: '/lookup/support-users',
        Managers: '/lookup/managers',
    },
}

export default ApiEndPoints