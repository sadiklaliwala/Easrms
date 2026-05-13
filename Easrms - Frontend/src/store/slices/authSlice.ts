import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

interface AuthUser {
    userId: string
    fullName: string
    email: string
    roleName: string
    managerId: string | null
}

interface AuthState {
    user: AuthUser | null
    isAuthenticated: boolean
}

const initialState: AuthState = {
    user: null,
    isAuthenticated: false
}

const authSlice = createSlice({
    name: 'auth',
    initialState,
    reducers: {
        setCredentials: (state, action: PayloadAction<AuthUser>) => {
            state.user = action.payload
            state.isAuthenticated = true
        },
        clearCredentials: (state) => {
            state.user = null
            state.isAuthenticated = false
        },
        updateUser: (state, action: PayloadAction<Partial<AuthUser>>) => {
            if (state.user) {
                state.user = { ...state.user, ...action.payload }
            }
        }
    }
})

export const { setCredentials, clearCredentials, updateUser } = authSlice.actions
export default authSlice.reducer