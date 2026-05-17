// // ============================================================
// // AUTH TYPES
// // ============================================================

// // POST /api/auth/login - Request Body
// export interface LoginRequestDto {
//     email: string
//     password: string
// }

// // POST /api/auth/login - Response
// export interface LoginResponseDto {
//     userId: string
//     fullName: string
//     email: string
//     roleName: string
//     managerId: string
//     accessToken: string
//     refreshToken: string
// }

// // GET /api/auth/me - Response
// export interface CurrentUserDto {
//     userId: string
//     fullName: string
//     email: string
//     roleName: string
// }

// // POST /api/auth/refresh-token - Request Body
// export interface RefreshTokenRequestDto {
//     accessToken: string
//     refreshToken: string
// }

// // POST /api/auth/refresh-token - Response
// export interface RefreshTokenResponseDto {
//     accessToken: string
//     refreshToken: string
// }


export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface LoginResponseDto {
  userId: string;
  fullName: string;
  email: string;
  roleName: string;
  managerId: string | null;
  accessToken: string;
  refreshToken: string;
}

export interface CurrentUserDto {
  userId: string;
  fullName: string;
  email: string;
  roleName: string;
}

export interface RefreshTokenRequestDto {
  accessToken: string;
  refreshToken: string;
}

export interface RefreshTokenResponseDto {
  accessToken: string;
  refreshToken: string;
}