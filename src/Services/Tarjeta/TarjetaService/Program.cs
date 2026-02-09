using BankingApp.GrpcContracts.Logging;
using BankingApp.SharedKernel.Extensions;
using BankingApp.SharedKernel.Middleware;
using BankingApp.SharedKernel.Responses;
using TarjetaService.Application.Mapping;
using TarjetaService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddBankingSwagger("Tarjeta Service");
builder.Services.AddBankingCors();
builder.Services.AddSqlServerFactory();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddGrpcLoggingClient(builder.Configuration);
builder.Services.AddScoped<ITarjetaRepository, TarjetaRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("banking-ui");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/tarjetas/clientes/{clienteId:int}/principal", async (int clienteId, ITarjetaRepository repository, CancellationToken cancellationToken) =>
{
    var tarjeta = await repository.GetPrincipalByClienteAsync(clienteId, cancellationToken);

    if (tarjeta is null)
    {
        return Results.Json(new ErrorResponse
        {
            Message = "El cliente no posee tarjeta principal",
            TimestampUtc = DateTime.UtcNow
        }, statusCode: StatusCodes.Status404NotFound);
    }

    return Results.Ok(tarjeta.ToDto());
})
.RequireAuthorization()
.WithName("GetTarjetaPrincipal")
.WithOpenApi(op =>
{
    op.Summary = "Obtiene la tarjeta principal del cliente";
    return op;
});

app.MapGet("/tarjetas/cuentas/{cuentaId:int}", async (int cuentaId, ITarjetaRepository repository, CancellationToken cancellationToken) =>
{
    var tarjetas = await repository.GetByCuentaAsync(cuentaId, cancellationToken);
    return Results.Ok(tarjetas.ToDtoList());
})
.RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .AllowAnonymous();

app.Run();

public partial class Program
{
}
