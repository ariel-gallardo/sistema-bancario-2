# Aurora Banking Frontend

Aplicacion React + Vite que expone el home banking y los paneles administrativos del sandbox bancario. El estado global se resuelve con Redux Toolkit y las vistas se apoyan en MUI.

## Comandos principales

| Script | Descripción |
| --- | --- |
| `npm run dev` | Levanta el frontend en modo desarrollo con Vite. |
| `npm run build` | Compila TypeScript y genera artefactos listos para produccion. |
| `npm run preview` | Sirve la build generada para validaciones manuales. |
| `npm run lint` | Ejecuta ESLint sobre todo el código fuente. |
| `npm run test:e2e` | Ejecuta la suite de pruebas end-to-end con Playwright. |

## Pruebas end-to-end

La automatizacion de interfaz utiliza [Playwright](https://playwright.dev/) y maqueta todas las llamadas HTTP que normalmente irian a los microservicios.

1. Instalar dependencias del proyecto (`npm install`).
2. Descargar los navegadores de Playwright una sola vez: `npx playwright install`.
3. Ejecutar `npm run test:e2e`.

Las pruebas abren la app real en Vite, interceptan las APIs (`/api/auth/login`, `/api/cuentas/*`, `/api/tarjetas/*` y `/api/admin/*`) y validan:

- Inicio de sesión y render del dashboard.
- Manejo de credenciales invalidas.
- Flujos del panel administrador (overview + revision de solicitudes).
- Alta manual de movimientos en `/admin/movimientos`.

No es necesario levantar los servicios de backend para correr la suite UI.
