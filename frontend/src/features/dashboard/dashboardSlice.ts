import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import axios from 'axios';
import { apiConfig } from '../../config/api';
import type { RootState } from '../../store';
import type { MovimientoResponse, ResumenClienteResponse, TarjetaResponse } from '../../types/api';

interface DashboardState {
  resumen?: ResumenClienteResponse;
  tarjeta?: TarjetaResponse | null;
  movimientos: MovimientoResponse[];
  status: 'idle' | 'loading' | 'succeeded' | 'failed';
  error?: string;
  clienteId: number | null;
}

const initialState: DashboardState = {
  movimientos: [],
  status: 'idle',
  clienteId: null,
};

const buildAuthHeaders = (token: string) => ({
  Authorization: `Bearer ${token}`,
});

const getDashboardErrorMessage = (error: unknown, fallback = 'No pudimos cargar tus datos') => {
  let message = fallback;
  if (axios.isAxiosError(error)) {
    if (typeof error.response?.data?.message === 'string') {
      message = error.response.data.message;
    } else if (error.response?.status === 401) {
      message = 'Tu sesión expiró, volvé a iniciar sesión';
    }
  }
  return message;
};

interface DashboardPayload {
  resumen: ResumenClienteResponse;
  tarjeta: TarjetaResponse | null;
  movimientos: MovimientoResponse[];
  clienteId: number;
}

export const fetchDashboardData = createAsyncThunk<
  DashboardPayload,
  number,
  { state: RootState; rejectValue: string }
>('dashboard/fetch', async (clienteId, { getState, rejectWithValue }) => {
  const token = getState().auth.token;
  if (!token) {
    return rejectWithValue('Token no disponible');
  }

  if (!clienteId || Number.isNaN(clienteId)) {
    return rejectWithValue('Necesitamos un cliente válido para consultar el resumen');
  }

  try {
    const headers = buildAuthHeaders(token);
    const resumenResponse = await axios.get<ResumenClienteResponse>(
      `${apiConfig.clientes}/clientes/${clienteId}/resumen`,
      { headers },
    );

    let tarjeta: TarjetaResponse | null = null;
    let movimientos: MovimientoResponse[] = [];

    try {
      const tarjetaResponse = await axios.get<TarjetaResponse>(
        `${apiConfig.tarjetas}/tarjetas/clientes/${clienteId}/principal`,
        { headers },
      );
      tarjeta = tarjetaResponse.data;

      const movimientosResponse = await axios.get<MovimientoResponse[]>(
        `${apiConfig.movimientos}/movimientos/tarjetas/${tarjeta.id}?take=5`,
        { headers },
      );
      movimientos = movimientosResponse.data;
    } catch (cardError) {
      if (!axios.isAxiosError(cardError) || cardError.response?.status !== 404) {
        throw cardError;
      }
    }

    return {
      resumen: resumenResponse.data,
      tarjeta,
      movimientos,
      clienteId,
    };
  } catch (error) {
    return rejectWithValue(getDashboardErrorMessage(error));
  }
});

const dashboardSlice = createSlice({
  name: 'dashboard',
  initialState,
  reducers: {
    resetDashboard: () => initialState,
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchDashboardData.pending, (state) => {
        state.status = 'loading';
        state.error = undefined;
      })
      .addCase(fetchDashboardData.fulfilled, (state, action) => {
        state.status = 'succeeded';
        state.resumen = action.payload.resumen;
        state.tarjeta = action.payload.tarjeta;
        state.movimientos = action.payload.movimientos;
        state.error = undefined;
        state.clienteId = action.payload.clienteId;
      })
      .addCase(fetchDashboardData.rejected, (state, action) => {
        state.status = 'failed';
        state.error = action.payload ?? action.error.message ?? 'Error inesperado';
        state.movimientos = [];
      });
  },
});

export const { resetDashboard } = dashboardSlice.actions;
export default dashboardSlice.reducer;
