# Sistema Bancario 3 (Aspire .NET 8)

Solución demostrativa basada en microservicios para exponer autenticación, clientes/cuentas, tarjetas, movimientos y un microservicio dedicado a logs mediante gRPC. Incluye:

- **Aspire AppHost** para orquestar los servicios.
- **Microservicios .NET 8** (Minimal APIs) con JWT, Swagger con ejemplos y manejo de errores estandarizado.
- **gRPC Logging Service** para recolectar excepciones y centralizarlas.
- **SQL Server + Dapper** con stored procedures para el resumen de cliente.
- **Frontend React 15.6.1 + Material UI** listo para consultar el resumen de cliente.

## Estructura

```
├── src/
│   ├── BankingApp.AppHost/            # Orquestación Aspire
│   ├── Services/
│   │   ├── AuthService/
│   │   ├── ClienteCuentaService/
│   │   ├── TarjetaService/
│   │   ├── MovimientoService/
│   │   └── LoggingService/
│   ├── Shared/
│   │   ├── BankingApp.SharedKernel/   # DTOs, middleware, DI helpers
│   │   └── BankingApp.GrpcContracts/  # Cliente gRPC reutilizable
│   └── Protos/logging.proto
├── database/
│   ├── auth/schema.sql                # Usuarios + credenciales AuthDb
│   ├── clientecuenta/schema.sql       # Clientes, cuentas, SP resumen
│   ├── tarjeta/schema.sql             # Tarjetas por cuenta
│   └── movimiento/schema.sql          # Movimientos por tarjeta
├── frontend/
│   ├── index.html                     # React 15 + Material UI
│   └── styles.css
└── BankingApp.sln
```

## Requisitos

- .NET SDK 8.0 o superior (AppHost usa net10 target).
- SQL Server local (puede ser contenedor) con credenciales configuradas en `ConnectionStrings:SqlServer`.
- Node/npm opcional para servir el frontend (o usar cualquier servidor estático).

## Ejecución

1. **Base de datos**
  - Al ejecutar el AppHost, Aspire levanta un SQL Server contenedorizado y aplica automáticamente todos los scripts en `database/*/schema.sql` (uno por microservicio).
  - Si deseas inicializar una instancia externa manualmente, ejecuta cada script:
    ```bash
    sqlcmd -S localhost,1433 -U sa -P "P@ssw0rd!" -i database/auth/schema.sql
    sqlcmd -S localhost,1433 -U sa -P "P@ssw0rd!" -i database/clientecuenta/schema.sql
    sqlcmd -S localhost,1433 -U sa -P "P@ssw0rd!" -i database/tarjeta/schema.sql
    sqlcmd -S localhost,1433 -U sa -P "P@ssw0rd!" -i database/movimiento/schema.sql
    ```

2. **Levantar microservicios** (incluye logging gRPC y APIs):
   ```bash
   dotnet run --project src/BankingApp.AppHost/BankingApp.AppHost.csproj
   ```
   Aspire expondrá cada proyecto con puertos dinámicos; revisa la consola para ver las URLs.

3. **Frontend React 15.6.1**
   - Abrir `frontend/index.html` directamente en un navegador **o** servirlo:
     ```bash
     npx serve frontend
     ```
   - Ingresa la URL base del microservicio ClienteCuenta (`/clientes/{id}/resumen`) y el token JWT emitido por `AuthService` (`/auth/token`).

## APIs Clave

- **AuthService**
  - `POST /auth/token` → entregar `username/password` (ej: `maria` / `P@ssw0rd`) y recibir JWT.
  - `GET /auth/me` → validar token.
  - Usuarios de prueba sembrados: `maria`, `juan`, `admin`, `tester1`, `tester2` (las dos últimas comparten contraseña `Tester#2026`).
- **ClienteCuentaService**
  - `GET /clientes/{clienteId}/resumen` → ejecuta `usp_GetClienteResumen` con Dapper, maneja caso sin tarjeta principal.
- **TarjetaService**
  - `GET /tarjetas/clientes/{clienteId}/principal`
  - `GET /tarjetas/cuentas/{cuentaId}`
- **MovimientoService**
  - `GET /movimientos/tarjetas/{tarjetaId}?take=5`
- **LoggingService**
  - gRPC `LogCollector.SendErrorLog` para recopilar excepciones de todos los microservicios.

Todas las APIs están aseguradas con JWT (excepto `/health` y `/auth/token`). Swagger con ejemplos se encuentra en `/swagger` para cada servicio.

## Manejo de errores y logs

- `ErrorHandlingMiddleware` intercepta excepciones, normaliza la respuesta (`ErrorResponse`) y envía el detalle al microservicio de logs vía gRPC (configurable en `Logging:GrpcEndpoint`).
- Cada servicio configura `ServiceInfo:Name` para etiquetar el origen del error.

## Stored Procedure solicitado

Se crea automáticamente dentro de [database/clientecuenta/schema.sql](database/clientecuenta/schema.sql) como `dbo.usp_GetClienteResumen`. Devuelve:
- Saldo de la cuenta principal (`SaldoCuentaPrincipal`).
- Últimos 5 movimientos de la tarjeta principal (si existe). Si no hay tarjeta principal, la segunda grilla se retorna vacía.

## Credenciales de SQL por microservicio

- AuthDb → login `auth_app` / `Auth#2026`
- ClienteCuentaDb → login `clientes_app` / `Cliente#2026`
- TarjetaDb → login `tarjeta_app` / `Tarjeta#2026`
- MovimientoDb → login `movimientos_app` / `Movimiento#2026`

## React + Material UI

`frontend/index.html` usa React 15.6.1, Material UI 0.20 y Babel standalone para permitir JSX sin bundler. El usuario ingresa `ClienteId`, `API Base` y `Token JWT`, y la interfaz:
- Muestra saldo principal en una tarjeta destacada.
- Lista los movimientos en una tabla (`Table` de Material UI) o explica que no hay tarjeta principal.
- Incluye validaciones básicas y mensajes mediante `Snackbar`.

## Próximos pasos sugeridos

1. Contenerizar SQL Server y servicios para stack completo en Aspire (SqlServerResource, React Static Web App, etc.).
2. Agregar integración real de gRPC streaming para auditorías o dashboards en tiempo real.
3. Implementar Redux u otra capa de estado global en el frontend si se complejiza.
