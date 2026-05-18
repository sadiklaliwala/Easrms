// import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

// interface AuthUser {
//     userId: string
//     fullName: string
//     email: string
//     roleName: string
//     managerId: string | null
// }

// interface AuthState {
//     user: AuthUser | null
//     isAuthenticated: boolean
// }

// const initialState: AuthState = {
//     user: null,
//     isAuthenticated: false
// }

// const authSlice = createSlice({
//     name: 'auth',
//     initialState,
//     reducers: {
//         setCredentials: (state, action: PayloadAction<AuthUser>) => {
//             state.user = action.payload
//             state.isAuthenticated = true
//         },
//         clearCredentials: (state) => {
//             state.user = null
//             state.isAuthenticated = false
//         },
//         updateUser: (state, action: PayloadAction<Partial<AuthUser>>) => {
//             if (state.user) {
//                 state.user = { ...state.user, ...action.payload }
//             }
//         }
//     }
// })

// export const { setCredentials, clearCredentials, updateUser } = authSlice.actions
// export default authSlice.reducer

import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import { type LoginResponseDto } from "../../types/auth.types";

interface AuthState {
  userId: string | null;
  fullName: string | null;
  email: string | null;
  roleName: string | null;
  managerId: string | null;
  isAuthenticated: boolean;
}

const initialState: AuthState = {
  userId: null,
  fullName: null,
  email: null,
  roleName: null,
  managerId: null,
  isAuthenticated: false,
};

const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    setCredentials: (state, action: PayloadAction<LoginResponseDto>) => {
      state.userId = action.payload.userId;
      state.fullName = action.payload.fullName;
      state.email = action.payload.email;
      state.roleName = action.payload.roleName;
      state.managerId = action.payload.managerId ?? null;
      state.isAuthenticated = true;
    },
    clearCredentials: (state) => {
      state.userId = null;
      state.fullName = null;
      state.email = null;
      state.roleName = null;
      state.managerId = null;
      state.isAuthenticated = false;
    },
  },
});

export const { setCredentials, clearCredentials } = authSlice.actions;
export default authSlice.reducer;
