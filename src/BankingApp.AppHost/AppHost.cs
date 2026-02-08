using System.IO;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var scriptsRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "database"));
string LoadScript(string relativePath) => File.ReadAllText(Path.Combine(scriptsRoot, relativePath));
const int SqlServerHostPort = 14333;
string BuildSqlConnectionString(string database, string user, string password) =>
	$"Server=localhost,{SqlServerHostPort};Database={database};User Id={user};Password={password};TrustServerCertificate=True";

var sqlServer = builder.AddSqlServer("central-sql")
	.WithHostPort(SqlServerHostPort)
	.WithDataVolume();

var authDb = sqlServer.AddDatabase("auth-db", "AuthDb")
	.WithCreationScript(LoadScript(Path.Combine("auth", "schema.sql")));

var clientesDb = sqlServer.AddDatabase("clientescuenta-db", "ClientesCuentaDb")
	.WithCreationScript(LoadScript(Path.Combine("clientecuenta", "schema.sql")));

var tarjetaDb = sqlServer.AddDatabase("tarjeta-db", "TarjetasDb")
	.WithCreationScript(LoadScript(Path.Combine("tarjeta", "schema.sql")));

var movimientoDb = sqlServer.AddDatabase("movimiento-db", "MovimientosDb")
	.WithCreationScript(LoadScript(Path.Combine("movimiento", "schema.sql")));

var authConnectionString = BuildSqlConnectionString("AuthDb", "auth_app", "Auth#2026");
var clientesConnectionString = BuildSqlConnectionString("ClienteCuentaDb", "clientes_app", "Cliente#2026");

var logging = builder.AddProject<LoggingService>("logging-service")
	.WithEnvironment("ServiceInfo__Name", "LoggingService");

builder.AddProject<AuthService>("auth-service")
	.WithReference(logging)
	.WithReference(authDb)
    .WaitFor(authDb)
    .WithEnvironment("ConnectionStrings__SqlServer", authConnectionString)
	.WithEnvironment("ServiceInfo__Name", "AuthService");

builder.AddProject<ClienteCuentaService>("clientes-service")
	.WithReference(logging)
	.WithReference(clientesDb)
    .WaitFor(clientesDb)
    .WithEnvironment("ConnectionStrings__SqlServer", clientesConnectionString)
	.WithEnvironment("ServiceInfo__Name", "ClienteCuentaService");

builder.AddProject<TarjetaService>("tarjetas-service")
	.WithReference(logging)
	.WithReference(tarjetaDb)
    .WaitFor(tarjetaDb)
    .WithEnvironment("ConnectionStrings__SqlServer", tarjetaDb.Resource.ConnectionStringExpression)
	.WithEnvironment("ServiceInfo__Name", "TarjetaService");

builder.AddProject<MovimientoService>("movimientos-service")
	.WithReference(logging)
	.WithReference(movimientoDb)
	.WaitFor(movimientoDb)
	.WithEnvironment("ConnectionStrings__SqlServer", movimientoDb.Resource.ConnectionStringExpression)
	.WithEnvironment("ServiceInfo__Name", "MovimientoService");

builder.Build().Run();
