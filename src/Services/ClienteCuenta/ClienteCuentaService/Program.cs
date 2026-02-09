using BankingApp.GrpcContracts.Logging;
using BankingApp.SharedKernel.Extensions;
using BankingApp.SharedKernel.Middleware;
using BankingApp.SharedKernel.Responses;
using ClienteCuentaService.Application.Mapping;
using ClienteCuentaService.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddBankingSwagger("Cliente & Cuenta Service");
builder.Services.AddBankingCors();
builder.Services.AddSqlServerFactory();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddGrpcLoggingClient(builder.Configuration);
builder.Services.AddScoped<IClienteCuentaRepository, ClienteCuentaRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("banking-ui");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapGet("/clientes/{clienteId:int}/resumen", async (int clienteId, IClienteCuentaRepository repository, CancellationToken cancellationToken) =>
{
    var resumen = await repository.GetResumenClienteAsync(clienteId, cancellationToken);
    if (resumen is null)
    {
        return Results.NotFound(new ErrorResponse
        {
            Message = "El cliente no posee cuenta principal",
            TimestampUtc = DateTime.UtcNow
        });
    }

    return Results.Ok(resumen.ToDto());
})
.RequireAuthorization()
.WithName("GetResumenCliente")
.WithOpenApi(operation =>
{
    operation.Summary = "Resumen financiero del cliente";
    operation.Responses["200"].Description = "Resumen obtenido";
    if (operation.Responses.TryGetValue("200", out var okResponse) && okResponse.Content.TryGetValue("application/json", out var media))
    {
        media.Example = new Microsoft.OpenApi.Any.OpenApiString("{\"saldoCuentaPrincipal\":157500.25,\"movimientos\":[{\"fecha\":\"2024-04-20T00:00:00Z\",\"monto\":-9500,\"descripcion\":\"Compra supermercado\"}]} ");
    }
    return operation;
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .AllowAnonymous();


app.Run();

public partial class Program
{
}
