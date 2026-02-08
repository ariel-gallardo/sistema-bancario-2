using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var scriptsRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "database"));
string LoadScript(string relativePath) => File.ReadAllText(Path.Combine(scriptsRoot, relativePath));
const string SqlServerPassword = "Banking#2026";

var sqlPassword = builder.AddParameter("sql-password", SqlServerPassword);

var sqlServer = builder.AddSqlServer("central-sql",sqlPassword)
	.WithExternalHttpEndpoints()
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_PID", "Developer")
    .WithDataVolume();

var authDb = sqlServer.AddDatabase("auth-db", "AuthDb")
	.WithCreationScript(LoadScript(Path.Combine("auth", "schema.sql")));

var clientesDb = sqlServer.AddDatabase("clientescuenta-db", "ClienteCuentaDb")
	.WithCreationScript(LoadScript(Path.Combine("clientecuenta", "schema.sql")));

var tarjetaDb = sqlServer.AddDatabase("tarjeta-db", "TarjetaDb")
	.WithCreationScript(LoadScript(Path.Combine("tarjeta", "schema.sql")));

var movimientoDb = sqlServer.AddDatabase("movimiento-db", "MovimientoDb")
	.WithCreationScript(LoadScript(Path.Combine("movimiento", "schema.sql")));

var logging = builder.AddProject<LoggingService>("logging-service")
	.WithEnvironment("ServiceInfo__Name", "LoggingService");

var authService = builder.AddProject<AuthService>("auth-service")
	.WithReference(logging)
	.WithReference(authDb)
    .WaitFor(authDb)
    .WithEnvironment("ConnectionStrings__SqlServer", authDb.Resource.ConnectionStringExpression)
	.WithEnvironment("ServiceInfo__Name", "AuthService");

var clientesService = builder.AddProject<ClienteCuentaService>("clientes-service")
	.WithReference(logging)
	.WithReference(clientesDb)
    .WaitFor(clientesDb)
    .WithEnvironment("ConnectionStrings__SqlServer", clientesDb.Resource.ConnectionStringExpression)
	.WithEnvironment("ServiceInfo__Name", "ClienteCuentaService");

var tarjetasService = builder.AddProject<TarjetaService>("tarjetas-service")
	.WithReference(logging)
	.WithReference(tarjetaDb)
    .WaitFor(tarjetaDb)
    .WithEnvironment("ConnectionStrings__SqlServer", tarjetaDb.Resource.ConnectionStringExpression)
	.WithEnvironment("ServiceInfo__Name", "TarjetaService");

var movimientosService = builder.AddProject<MovimientoService>("movimientos-service")
	.WithReference(logging)
	.WithReference(movimientoDb)
	.WaitFor(movimientoDb)
	.WithEnvironment("ConnectionStrings__SqlServer", movimientoDb.Resource.ConnectionStringExpression)
	.WithEnvironment("ServiceInfo__Name", "MovimientoService");

builder.AddJavaScriptApp("frontend", "../../frontend", "dev")
	.WithReference(authService)
	.WithReference(clientesService)
	.WithReference(tarjetasService)
	.WithReference(movimientosService)
	.WithEnvironment("VITE_AUTH_URL", authService.GetEndpoint("http"))
	.WithEnvironment("VITE_CLIENTES_URL", clientesService.GetEndpoint("http"))
	.WithEnvironment("VITE_TARJETAS_URL", tarjetasService.GetEndpoint("http"))
	.WithEnvironment("VITE_MOVIMIENTOS_URL", movimientosService.GetEndpoint("http"))
	.WithEnvironment("VITE_LOGS_URL", $"{logging.GetEndpoint("http")}");

builder.Build().Run();
