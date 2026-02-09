using BankingApp.GrpcContracts.Logging;
using BankingApp.SharedKernel.Extensions;
using BankingApp.SharedKernel.Middleware;
using MovimientoService.Application.Mapping;
using MovimientoService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddBankingSwagger("Movimiento Service");
builder.Services.AddBankingCors();
builder.Services.AddSqlServerFactory();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddGrpcLoggingClient(builder.Configuration);
builder.Services.AddScoped<IMovimientoRepository, MovimientoRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("banking-ui");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/movimientos/clientes/{clienteId:int}", async (int clienteId, int? take, IMovimientoRepository repository, CancellationToken cancellationToken) =>
{
    var limit = Math.Clamp(take ?? 5, 1, 50);
    var movimientos = await repository.GetByClienteAsync(clienteId, limit, cancellationToken);
    return Results.Ok(movimientos.ToDtoList());
})
.RequireAuthorization()
.WithName("GetMovimientosPorCliente")
.WithOpenApi(op =>
{
    op.Summary = "Obtiene los últimos movimientos de la tarjeta principal del cliente";
    op.Parameters[1].Description = "Cantidad máxima de movimientos (por defecto 5)";
    return op;
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .AllowAnonymous();

app.Run();

public partial class Program
{
}
