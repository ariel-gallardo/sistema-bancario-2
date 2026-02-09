import type { Page, Route } from '@playwright/test';
import {
  accountById,
  adminOverview,
  adminPendingRegistrations,
  cardMovementsById,
  cardSummaries,
  demoCredentials,
  mockLoginResponse,
  primaryAccount,
} from './mockData';

const clientesOrigin = 'http://localhost:65534';
const cuentasOrigin = 'http://localhost:65532';
const tarjetasOrigin = 'http://localhost:65515';

const corsHeaders = {
  'Access-Control-Allow-Origin': '*',
  'Access-Control-Allow-Headers': '*',
  'Access-Control-Allow-Methods': 'GET,POST,PUT,DELETE,OPTIONS',
};

const fulfillJson = async (route: Route, body: unknown, status = 200) => {
  await route.fulfill({
    status,
    body: JSON.stringify(body),
    headers: {
      'Content-Type': 'application/json',
      ...corsHeaders,
    },
  });
};

const fulfillEmpty = async (route: Route, status = 204) => {
  await route.fulfill({
    status,
    headers: {
      ...corsHeaders,
    },
  });
};

export interface ApiMockOptions {
  loginErrorMessage?: string;
}

export const installApiMocks = async (page: Page, options?: ApiMockOptions) => {
  await page.route(/http:\/\/localhost:(65534|65532|65515)\/api\/.*$/, async (route) => {
    const request = route.request();
    const method = request.method();
    const url = new URL(request.url());

    if (method === 'OPTIONS') {
      await fulfillEmpty(route);
      return;
    }

    if (url.origin === clientesOrigin && url.pathname === '/api/auth/login' && method === 'POST') {
      if (options?.loginErrorMessage) {
        await fulfillJson(route, { message: options.loginErrorMessage }, 401);
      } else {
        await fulfillJson(route, { ...mockLoginResponse });
      }
      return;
    }

    if (url.origin === clientesOrigin && url.pathname === '/api/admin/overview' && method === 'GET') {
      await fulfillJson(route, adminOverview);
      return;
    }

    if (url.origin === clientesOrigin && url.pathname === '/api/admin/registros/pendientes' && method === 'GET') {
      await fulfillJson(route, adminPendingRegistrations);
      return;
    }

    if (
      url.origin === clientesOrigin &&
      method === 'POST' &&
      url.pathname.startsWith('/api/admin/registros/') &&
      url.pathname.endsWith('/revisar')
    ) {
      await fulfillEmpty(route);
      return;
    }

    if (url.origin === cuentasOrigin && url.pathname === '/api/cuentas/principal' && method === 'GET') {
      await fulfillJson(route, primaryAccount);
      return;
    }

    if (url.origin === cuentasOrigin && method === 'GET' && url.pathname.startsWith('/api/cuentas/')) {
      const segments = url.pathname.split('/');
      const accountId = segments[segments.length - 1];
      const payload = accountById[accountId];
      if (payload) {
        await fulfillJson(route, payload);
        return;
      }
    }

    if (url.origin === cuentasOrigin && method === 'PUT' && url.pathname.endsWith('/favorita')) {
      await fulfillEmpty(route);
      return;
    }

    if (url.origin === cuentasOrigin && method === 'DELETE' && url.pathname === '/api/cuentas/favorita') {
      await fulfillEmpty(route);
      return;
    }

    if (url.origin === tarjetasOrigin && method === 'GET' && url.pathname.startsWith('/api/tarjetas/cuentas/')) {
      const accountId = url.pathname.split('/').pop();
      const cards = cardSummaries.filter((card) => card.cuentaId === accountId);
      await fulfillJson(route, cards);
      return;
    }

    if (url.origin === tarjetasOrigin && url.pathname.startsWith('/api/tarjetas/')) {
      const segments = url.pathname.split('/');
      const tarjetaId = segments[3];

      if (!tarjetaId) {
        await route.fallback();
        return;
      }

      if (method === 'GET' && url.pathname.endsWith('/movimientos')) {
        const payload = cardMovementsById[tarjetaId];
        await fulfillJson(route, payload ?? cardMovementsById[cardSummaries[0].tarjetaId]);
        return;
      }

      if (method === 'POST' && url.pathname.endsWith('/movimientos')) {
        await fulfillJson(route, { ok: true }, 201);
        return;
      }
    }

    await route.fallback();
  });
};

export const loginThroughUi = async (page: Page) => {
  await page.goto('/login');
  await page.getByLabel('Tu número de documento').fill(demoCredentials.email);
  await page.getByLabel('Tu nombre de usuario').fill(demoCredentials.email);
  await page.getByLabel('Tu clave').fill(demoCredentials.password);
  await page.getByRole('button', { name: /ingresar/i }).click();
};
