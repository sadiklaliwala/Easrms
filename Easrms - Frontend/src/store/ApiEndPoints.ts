// const ApiEndPoints = {
//     baseUrl: 'https://localhost:7252/api',

//     Auth: {
//         Login: '/auth/login',
//         Logout: '/auth/logout',
//         Me: '/auth/me',
//         RefreshToken: '/auth/refresh-token',
//         RevokeToken: '/auth/revoke-token',
//     },

//     Users: {
//         GetAll: '/users',
//         GetById: (id: string) => `/users/${id}`,
//         Create: '/users',
//         Update: (id: string) => `/users/${id}`,
//         ToggleStatus: (id: string) => `/users/${id}/activate-deactivate`,
//     },

//     Categories: {
//         GetAll: '/categories',
//         GetById: (id: string) => `/categories/${id}`,
//         Create: '/categories',
//         Update: (id: string) => `/categories/${id}`,
//         ToggleStatus: (id: string) => `/categories/${id}/activate-deactivate`,
//     },

//     Requests: {
//         GetAll: '/requests',
//         GetById: (id: string) => `/requests/${id}`,
//         Create: '/requests',
//         Approve: (id: string) => `/requests/${id}/approval`,
//         Assign: (id: string) => `/requests/${id}/assign`,
//         UpdateStatus: (id: string) => `/requests/${id}/status`,
//         Close: (id: string) => `/requests/${id}/close`,
//     },

//     Comments: {
//         GetAll: (requestId: string) => `/requests/${requestId}/comments`,
//         Add: (requestId: string) => `/requests/${requestId}/comments`,
//     },

//     History: {
//         GetAll: (requestId: string) => `/requests/${requestId}/history`,
//     },

//     Dashboard: {
//         Summary: '/dashboard/summary',
//     },

//     Lookup: {
//         SupportUsers: '/lookup/support-users',
//         Managers: '/lookup/managers',
//     },
// }

// export default ApiEndPoints

const ApiEndPoints = {
  // Auth
  AUTH: {
    LOGIN: '/api/Auth/login',
    LOGOUT: '/api/Auth/logout',
    ME: '/api/Auth/me',
    REFRESH_TOKEN: '/api/Auth/refresh-token',
    REVOKE_TOKEN: '/api/Auth/revoke-token',
    OAUTH_LOGIN: '/api/Auth/oauth-login',
    LINK_PROVIDER: '/api/Auth/link-provider',
    UNLINK_PROVIDER: '/api/Auth/unlink-provider',
    LINKED_PROVIDERS: '/api/Auth/linked-providers',
  },

  // Users
  USERS: {
    BASE: '/api/User',
    BY_ID: (id: string) => `/api/User/${id}`,
    TOGGLE: (id: string) => `/api/User/${id}/activate-deactivate`,
  },

  // Categories
  CATEGORIES: {
    BASE: '/api/Category',
    BY_ID: (id: string) => `/api/Category/${id}`,
    TOGGLE: (id: string) => `/api/Category/${id}/activate-deactivate`,
  },

  // Requests
  REQUESTS: {
    BASE: '/api/Request',
    BY_ID: (id: string) => `/api/Request/${id}`,
    APPROVAL: (id: string) => `/api/Request/${id}/approval`,
    ASSIGN: (id: string) => `/api/Request/${id}/assign`,
    STATUS: (id: string) => `/api/Request/${id}/status`,
    CLOSE: (id: string) => `/api/Request/${id}/close`,
  },

  // Comments
  COMMENTS: {
    BASE: (requestId: string) => `/api/requests/${requestId}/comments`,
    HISTORY: (requestId: string) => `/api/requests/${requestId}/history`,
  },

  // Dashboard
  DASHBOARD: {
    SUMMARY: '/api/Dashboard/summary',
  },

  // Lookup
  LOOKUP: {
    SUPPORT_USERS: '/api/Lookup/support-users',
    MANAGERS: '/api/Lookup/managers',
  },

  // Cloudinary
  CLOUDINARY: {
    SIGN: '/api/Cloudinary/sign',
  },

  // Export
  EXPORT: {
    REQUESTS_EXCEL: '/api/export/requests/excel',
    REQUESTS_PDF: '/api/export/requests/pdf',
    REQUEST_DETAIL_EXCEL: (id: string) => `/api/export/requests/${id}/excel`,
    REQUEST_DETAIL_PDF: (id: string) => `/api/export/requests/${id}/pdf`,
  },
  PROFILE: '/api/profile',
  PROFILE_SEND_OTP: '/api/profile/send-otp',
  PROFILE_VERIFY_OTP: '/api/profile/verify-otp',
  PROFILE_CHANGE_PASSWORD: '/api/profile/change-password',
  AUTH_SEND_OTP: '/api/auth/send-otp',
  AUTH_VERIFY_OTP: '/api/auth/verify-otp',
  AUTH_RESET_PASSWORD: '/api/auth/reset-password',
};

export default ApiEndPoints;