const fallbackUrls = {
  auth: 'http://localhost:5101',
  clientes: 'http://localhost:5102',
  tarjetas: 'http://localhost:5103',
  movimientos: 'http://localhost:5104',
  logs: 'http://localhost:5105/graphql',
};

export const apiConfig = {
  auth: import.meta.env.VITE_AUTH_URL ?? fallbackUrls.auth,
  clientes: import.meta.env.VITE_CLIENTES_URL ?? fallbackUrls.clientes,
  tarjetas: import.meta.env.VITE_TARJETAS_URL ?? fallbackUrls.tarjetas,
  movimientos: import.meta.env.VITE_MOVIMIENTOS_URL ?? fallbackUrls.movimientos,
  logs: import.meta.env.VITE_LOGS_URL ?? fallbackUrls.logs,
};

export type DemoUserRole = 'admin' | 'cliente';

export interface DemoUserCredential {
  role: DemoUserRole;
  username: string;
  password: string;
  description: string;
  clienteId?: number;
}

export const demoUsers: DemoUserCredential[] = [
  {
    role: 'cliente',
    username: 'maria',
    password: 'P@ssw0rd',
    clienteId: 1,
    description: 'Cliente con tarjeta principal',
  },
  {
    role: 'cliente',
    username: 'juan',
    password: 'P@ssw0rd',
    clienteId: 2,
    description: 'Cliente PyME',
  },
  {
    role: 'admin',
    username: 'admin',
    password: 'Admin2024',
    description: 'Operaciones y monitoreo',
  },
];

export const demoCredentials = demoUsers[0];
