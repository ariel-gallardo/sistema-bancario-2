import type {
  AccountSummaryResponse,
  AdminOverviewResponse,
  ClienteRegistroSummary,
  LoginResponse,
  TarjetaMovimientosResponse,
  TarjetaResumenResponse,
} from '../../../src/types/api';

export const mockLoginResponse: LoginResponse = {
  clienteId: 'cliente-001',
  cuentaPrincipalId: 'cta-001',
  tarjetaPrincipalId: 'card-001',
  nombre: 'Ariel Gallardo',
  email: 'demo@aurorabank.com',
  token: 'jwt-demo-token',
  expiraUtc: '2025-02-15T10:00:00Z',
  esAdministrador: true,
};

export const demoCredentials = {
  email: mockLoginResponse.email,
  password: 'B4nco$123',
};

export const primaryAccount: AccountSummaryResponse = {
  cuentaId: 'cta-001',
  alias: 'Cuenta sueldo',
  banco: 'Aurora Bank',
  numero: '123-456789/0',
  moneda: 'ARS',
  saldoActual: 1850000.55,
  saldoDisponible: 1720000.25,
  ultimaActualizacion: '2025-02-10T14:35:00Z',
  esPrincipal: true,
  esFavorita: true,
  otrasCuentas: [
    {
      cuentaId: 'cta-002',
      alias: 'Cuenta PyME',
      moneda: 'ARS',
      saldoActual: 890000.0,
      esPrincipal: false,
      esFavorita: false,
    },
  ],
};

export const secondaryAccount: AccountSummaryResponse = {
  ...primaryAccount,
  cuentaId: 'cta-002',
  alias: 'Cuenta PyME',
  saldoActual: 890000.0,
  saldoDisponible: 860000.0,
  esPrincipal: false,
  esFavorita: false,
  otrasCuentas: [
    {
      cuentaId: 'cta-001',
      alias: primaryAccount.alias,
      moneda: primaryAccount.moneda,
      saldoActual: primaryAccount.saldoActual,
      esPrincipal: true,
      esFavorita: true,
    },
  ],
};

export const accountById: Record<string, AccountSummaryResponse> = {
  [primaryAccount.cuentaId]: primaryAccount,
  [secondaryAccount.cuentaId]: secondaryAccount,
};

export const cardSummaries: TarjetaResumenResponse[] = [
  {
    tarjetaId: 'card-001',
    cuentaId: primaryAccount.cuentaId,
    marca: 'Visa',
    numeroEnmascarado: '**** 4321',
    limite: 800000,
    saldoUtilizado: 220000,
    disponible: 580000,
    esPrincipal: true,
  },
  {
    tarjetaId: 'card-002',
    cuentaId: primaryAccount.cuentaId,
    marca: 'Mastercard',
    numeroEnmascarado: '**** 7788',
    limite: 650000,
    saldoUtilizado: 150000,
    disponible: 500000,
    esPrincipal: false,
  },
];

const baseMovements: TarjetaMovimientosResponse = {
  tarjetaId: 'card-001',
  cuentaId: primaryAccount.cuentaId,
  marca: 'Visa',
  numeroEnmascarado: '**** 4321',
  limite: 800000,
  saldoUtilizado: 220000,
  disponible: 580000,
  pagoMinimo: 125000,
  fechaCierre: '2025-02-20T00:00:00Z',
  fechaVencimiento: '2025-03-05T00:00:00Z',
  movimientos: [
    {
      movimientoId: 'mov-001',
      comercio: 'Serviclub Shell',
      descripcion: 'Combustible premium',
      categoria: 'Servicios',
      importe: 82000,
      fecha: '2025-02-01T10:00:00Z',
    },
    {
      movimientoId: 'mov-002',
      comercio: 'Mercado Aurora',
      descripcion: 'Compra mensual',
      categoria: 'Supermercado',
      importe: 135000,
      fecha: '2025-01-30T18:30:00Z',
    },
    {
      movimientoId: 'mov-003',
      comercio: 'TecnoPlus',
      descripcion: 'Periféricos',
      categoria: 'Tecnología',
      importe: 45000,
      fecha: '2025-01-28T12:15:00Z',
    },
    {
      movimientoId: 'mov-004',
      comercio: 'Restó Litoral',
      descripcion: 'Cena equipo',
      categoria: 'Gastronomía',
      importe: 60000,
      fecha: '2025-01-26T21:00:00Z',
    },
    {
      movimientoId: 'mov-005',
      comercio: 'ViajaYa',
      descripcion: 'Pasajes Air',
      categoria: 'Viajes',
      importe: 210000,
      fecha: '2025-01-24T08:00:00Z',
    },
  ],
};

export const cardMovementsById: Record<string, TarjetaMovimientosResponse> = {
  [cardSummaries[0].tarjetaId]: baseMovements,
  [cardSummaries[1].tarjetaId]: {
    ...baseMovements,
    tarjetaId: cardSummaries[1].tarjetaId,
    marca: 'Mastercard',
    numeroEnmascarado: cardSummaries[1].numeroEnmascarado,
    movimientos: baseMovements.movimientos.map((movement, index) => ({
      ...movement,
      movimientoId: `card-002-mov-${index}`,
      comercio: index % 2 === 0 ? 'DigitalOcean' : 'Helados Bruma',
      categoria: index % 2 === 0 ? 'Tecnología' : 'Gastronomía',
    })),
  },
};

export const adminOverview: AdminOverviewResponse = {
  totalClientes: 4820,
  solicitudesPendientes: 7,
  solicitudesRevisadas: 126,
  ultimaSolicitudUtc: '2025-02-11T09:45:00Z',
};

export const adminPendingRegistrations: ClienteRegistroSummary[] = [
  {
    registroId: 'reg-001',
    nombreCompleto: 'Valentina Castro',
    documento: '37.456.123',
    email: 'valentina@example.com',
    telefono: '+54 11 7900-1234',
    estado: 'Pendiente',
    creadoEnUtc: '2025-02-12T11:30:00Z',
  },
  {
    registroId: 'reg-002',
    nombreCompleto: 'Matías Benítez',
    documento: '29.880.421',
    email: 'matias@example.com',
    telefono: '+54 11 6234-5566',
    estado: 'Pendiente',
    creadoEnUtc: '2025-02-11T16:00:00Z',
  },
];
