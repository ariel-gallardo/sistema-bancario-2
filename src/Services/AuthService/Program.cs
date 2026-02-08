using System.Security.Claims;
using AuthService.Repositories;
using AuthService.Services;
using BankingApp.GrpcContracts.Logging;
using BankingApp.SharedKernel.Auth;
using BankingApp.SharedKernel.Extensions;
using BankingApp.SharedKernel.Middleware;
using BankingApp.SharedKernel.Responses;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery();

builder.Services.AddBankingSwagger("Auth Service");
builder.Services.AddBankingCors();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddGrpcLoggingClient(builder.Configuration);
builder.Services.AddSqlServerFactory();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("banking-ui");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapPost("/auth/token", async (LoginRequestDto request, TokenService tokenService, CancellationToken cancellationToken) =>
{
    var roles = await tokenService.TryValidateUserAsync(request, cancellationToken);
    if (roles is null)
    {
        var body = new ErrorResponse
        {
            Message = "Credenciales inválidas",
            TimestampUtc = DateTime.UtcNow
        };
        return Results.Json(body, statusCode: StatusCodes.Status401Unauthorized);
    }

    var response = tokenService.CreateToken(request.Username, roles);
    return Results.Ok(response);
})
.AllowAnonymous()
.WithName("IssueToken")
.WithOpenApi(operation =>
{
    operation.Summary = "Obtiene un token JWT";
    operation.RequestBody = new Microsoft.OpenApi.Models.OpenApiRequestBody
    {
        Content = new Dictionary<string, Microsoft.OpenApi.Models.OpenApiMediaType>
        {
            ["application/json"] = new()
            {
                Example = new Microsoft.OpenApi.Any.OpenApiString("{\"username\":\"maria\",\"password\":\"P@ssw0rd\"}")
            }
        }
    };
    return operation;
});

app.MapGet("/auth/me", (ClaimsPrincipal user) =>
{
    if (!user.Identity?.IsAuthenticated ?? true)
    {
        return Results.Unauthorized();
    }

    var roles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
    return Results.Ok(new
    {
        user.Identity!.Name,
        Roles = roles
    });
}).RequireAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .AllowAnonymous()
   .WithOpenApi(o =>
   {
       o.Summary = "Healthcheck";
       return o;
   });

app.Run();
