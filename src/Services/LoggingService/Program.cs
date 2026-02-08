
using LoggingService.GraphQL;
using LoggingService.Services;
using LoggingService.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceDiscovery();

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
builder.Services.AddSingleton<ILogStore>(_ => new InMemoryLogStore(1_000));
builder.Services
	.AddGraphQLServer()
	.AddQueryType<LogQueries>()
	.ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();

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
