export interface LoginResponse {
  token: string;
  expiresAt: string;
  roles: string[];
}

export interface MovimientoResumen {
  fecha: string;
  monto: number;
  descripcion: string;
}

export interface ResumenClienteResponse {
  saldoCuentaPrincipal: number;
  movimientos: MovimientoResumen[];
  tieneTarjetaPrincipal: boolean;
}

export interface TarjetaResponse {
  id: number;
  cuentaId: number;
  numero: string;
  esPrincipal: boolean;
}

export interface MovimientoResponse {
  id: number;
  tarjetaId: number;
  fecha: string;
  monto: number;
  descripcion: string;
}

export interface LogEntry {
  id: string;
  timestampUtc: string;
  serviceName: string;
  message: string;
  severity: string;
  requestPath: string;
  stackTrace: string;
  correlationId: string;
  payload: string;
}

export interface LogPage {
  page: number;
  pageSize: number;
  totalCount: number;
  items: LogEntry[];
}
