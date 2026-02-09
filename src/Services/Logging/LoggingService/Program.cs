
using BankingApp.SharedKernel.Extensions;
using LoggingService.GraphQL;
using LoggingService.Services;
using LoggingService.Storage;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var loggingConnectionString = builder.Configuration.GetConnectionString("logging-db")
	?? throw new InvalidOperationException("Connection string 'LoggingDb' was not found.");
builder.Services.AddSqlServerFactory();

builder.Services.AddDbContext<LoggingDbContext>(options =>
	options.UseSqlServer(loggingConnectionString));


builder.Services.AddScoped<ILogStore, SqlLogStore>();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
	options.AddPolicy("logs-ui", policy =>
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader());
});
builder.Services
	.AddGraphQLServer()
	.AddQueryType<LogQueries>()
	.ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

	using (var scope = app.Services.CreateScope())
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<LoggingDbContext>();
	}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("logs-ui");

app.MapGrpcService<LogCollectorService>();
app.MapGrpcReflectionService();
app.MapGraphQL("/graphql").RequireCors("logs-ui");

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("GetLoggingHealth")
   .WithOpenApi(operation =>
   {
	   operation.Summary = "Verifica el estado del microservicio de logs";
	   return operation;
   });


app.Run();
