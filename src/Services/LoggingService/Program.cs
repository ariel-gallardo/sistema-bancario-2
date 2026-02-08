using LoggingService.Services;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("logs-ui");

app.MapGrpcService<LogCollectorService>();
app.MapGrpcReflectionService();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
   .WithName("GetLoggingHealth")
   .WithOpenApi(operation =>
   {
	   operation.Summary = "Verifica el estado del microservicio de logs";
	   return operation;
   });

app.Run();
