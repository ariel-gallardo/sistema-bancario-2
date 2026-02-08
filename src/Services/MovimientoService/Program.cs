using BankingApp.GrpcContracts.Logging;
using BankingApp.SharedKernel.Extensions;
using BankingApp.SharedKernel.Middleware;
using MovimientoService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery();

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

app.MapGet("/movimientos/tarjetas/{tarjetaId:int}", async (int tarjetaId, int? take, IMovimientoRepository repository, CancellationToken cancellationToken) =>
{
    var limit = Math.Clamp(take ?? 5, 1, 50);
    var movimientos = await repository.GetByTarjetaAsync(tarjetaId, limit, cancellationToken);
    return Results.Ok(movimientos);
})
.RequireAuthorization()
.WithName("GetMovimientosPorTarjeta")
.WithOpenApi(op =>
{
    op.Summary = "Obtiene los últimos movimientos de la tarjeta";
    op.Parameters[1].Description = "Cantidad máxima de movimientos (por defecto 5)";
    return op;
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .AllowAnonymous();

app.Run();
