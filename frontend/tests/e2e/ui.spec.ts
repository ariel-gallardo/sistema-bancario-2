import { expect, test } from '@playwright/test';
import { installApiMocks, loginThroughUi } from './utils/apiMocks';
import {
  adminOverview,
  adminPendingRegistrations,
  cardMovementsById,
  cardSummaries,
  demoCredentials,
} from './utils/mockData';

const firstCardMovements = cardMovementsById[cardSummaries[0].tarjetaId].movimientos;

test('admin login renders dashboard widgets and movements', async ({ page }) => {
  await installApiMocks(page);
  await loginThroughUi(page);

  await expect(page).toHaveURL(/dashboard/);
  await expect(page.getByRole('heading', { name: /bienvenido/i })).toBeVisible();
  await expect(page.getByText('Saldo cuenta seleccionada')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Panel admin' })).toBeVisible();

  await expect(page.getByRole('heading', { name: 'Últimos movimientos' })).toBeVisible();
  await expect(page.getByText(firstCardMovements[0].comercio)).toBeVisible();

  await expect(page.getByText('Sos administrador.', { exact: false })).toBeVisible();
  await expect(page.getByText('Movimientos críticos')).toBeVisible();
});

test('login screen shows API errors when credentials fail', async ({ page }) => {
  await installApiMocks(page, { loginErrorMessage: 'Credenciales inválidas' });

  await page.goto('/login');
  await page.getByLabel('Tu número de documento').fill(demoCredentials.email);
  await page.getByLabel('Tu nombre de usuario').fill(demoCredentials.email);
  await page.getByLabel('Tu clave').fill('wrong pass');
  await page.getByRole('button', { name: /ingresar/i }).click();

  await expect(page.getByText('Credenciales inválidas')).toBeVisible();
  await expect(page).toHaveURL(/login/);
});

test('admin can review registrations in the dashboard', async ({ page }) => {
  await installApiMocks(page);
  await loginThroughUi(page);

  await page.getByRole('button', { name: 'Panel admin' }).click();
  await expect(page).toHaveURL(/\/admin$/);

  await expect(page.getByText('Indicadores principales')).toBeVisible();
  await expect(page.getByText(String(adminOverview.totalClientes))).toBeVisible();

  await page.getByRole('button', { name: 'Solicitudes de alta' }).click();
  const firstPending = adminPendingRegistrations[0];
  await expect(page.getByText(firstPending.nombreCompleto)).toBeVisible();

  await page.getByRole('button', { name: 'Marcar como revisado' }).first().click();
  await expect(page.getByText(firstPending.nombreCompleto)).toHaveCount(0);
});

test('admin movements panel registers manual expenses', async ({ page }) => {
  await installApiMocks(page);
  await loginThroughUi(page);

  await page.getByRole('button', { name: 'Movimientos críticos' }).click();
  await expect(page).toHaveURL(/\/admin\/movimientos/);
  await expect(page.getByRole('heading', { name: 'Panel administrativo de movimientos' })).toBeVisible();

  await page.getByLabel('Comercio').fill('Cafetería Aurora');
  await page.getByLabel('Descripción').fill('Refuerzo operativo');
  await page.getByLabel('Categoría').click();
  await page.getByRole('option', { name: 'Gastronomía' }).click();
  await page.getByLabel('Importe').fill('45000');
  await page.getByLabel('Fecha del movimiento').fill('2025-02-14');

  await page.getByRole('button', { name: 'Registrar movimiento' }).click();
  await expect(page.getByText('Movimiento registrado con éxito.')).toBeVisible();
});
