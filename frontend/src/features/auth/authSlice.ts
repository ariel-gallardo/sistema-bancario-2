import { createAsyncThunk, createSlice, type PayloadAction } from '@reduxjs/toolkit';
import axios from 'axios';
import { apiConfig } from '../../config/api';
import type { LoginResponse } from '../../types/api';
import { clearAuthState, loadAuthState, saveAuthState, type StoredAuthState } from '../../utils/authStorage';

export interface LoginPayload {
  username: string;
  password: string;
  clienteId?: number | null;
}

interface AuthSuccessPayload extends LoginResponse {
  username: string;
  clienteId: number | null;
}

interface AuthState {
  token: string | null;
  username?: string;
  clienteId: number | null;
  roles: string[];
  expiresAt?: string;
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error?: string;
}

const createDefaultState = (): AuthState => ({
  token: null,
  username: undefined,
  clienteId: null,
  roles: [],
  expiresAt: undefined,
  status: 'idle',
  error: undefined,
});

const persistedAuth = loadAuthState();

const initialState: AuthState = (() => {
  const baseState = createDefaultState();
  if (!persistedAuth) {
    return baseState;
  }

  return {
    ...baseState,
    token: persistedAuth.token,
    username: persistedAuth.username,
    clienteId: persistedAuth.clienteId ?? null,
    roles: persistedAuth.roles ?? [],
    expiresAt: persistedAuth.expiresAt,
  };
})();

const persistState = (state: AuthState) => {
  if (!state.token) {
    clearAuthState();
    return;
  }

  const payload: StoredAuthState = {
    token: state.token,
    username: state.username,
    clienteId: state.clienteId ?? null,
    roles: state.roles,
    expiresAt: state.expiresAt,
  };

  saveAuthState(payload);
};

export const login = createAsyncThunk<
  AuthSuccessPayload,
  LoginPayload,
  { rejectValue: string }
>('auth/login', async (payload, { rejectWithValue }) => {
  try {
    const { username, password, clienteId = null } = payload;
    const response = await axios.post<LoginResponse>(`${apiConfig.auth}/auth/token`, {
      username,
      password,
    });

    return {
      token: response.data.token,
      expiresAt: response.data.expiresAt,
      roles: response.data.roles,
      username,
      clienteId,
    };
  } catch (error) {
    let message = 'No pudimos iniciar sesión';
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 401) {
        message = 'Credenciales inválidas';
      } else if (typeof error.response?.data?.message === 'string') {
        message = error.response.data.message;
      }
    }
    return rejectWithValue(message);
  }
});

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    logout: () => {
      clearAuthState();
      return createDefaultState();
    },
    setClienteId: (state, action: PayloadAction<number | null>) => {
      state.clienteId = action.payload;
      persistState(state);
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(login.pending, (state) => {
        state.status = 'loading';
        state.error = undefined;
      })
      .addCase(login.fulfilled, (state, action: PayloadAction<AuthSuccessPayload>) => {
        state.status = 'succeeded';
        state.token = action.payload.token;
        state.expiresAt = action.payload.expiresAt;
        state.roles = action.payload.roles;
        state.username = action.payload.username;
        state.clienteId = action.payload.clienteId;
        persistState(state);
      })
      .addCase(login.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload ?? action.error.message ?? 'Error inesperado';
        state.token = null;
        state.username = undefined;
        state.roles = [];
        state.expiresAt = undefined;
        state.clienteId = null;
        clearAuthState();
      });
  },
});

export const { logout, setClienteId } = authSlice.actions;
export default authSlice.reducer;
