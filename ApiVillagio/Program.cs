using Microsoft.EntityFrameworkCore;
using ApiVillagio.Data;

var builder = WebApplication.CreateBuilder(args);

var strConn = builder.Environment.IsDevelopment()
	? builder.Configuration.GetConnectionString("strConnExterna")
	: builder.Configuration.GetConnectionString("strConnInterna");

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(strConn));

// Adicionando servidor ao container
builder.Services.AddControllers()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
		options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
	});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adicionando CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAll", policy =>
	{
		policy.AllowAnyOrigin()
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

var app = builder.Build();

// Configurando o pipeline de requisi��o HTTP
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
		options.RoutePrefix = string.Empty;
	});
}

app.UseHttpsRedirection();

// Usando CORS
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();