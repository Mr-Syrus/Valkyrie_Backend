using Microsoft.EntityFrameworkCore;
using valkyrie.Models;
using valkyrie.Сontrollers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Подключение PostgreSQL через EF Core
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var api = app.MapGroup("/api");
var auth = new Auth(app, api);
var companies = new Companies(app, api, auth);
var users = new Users(app, api, auth, companies);
var platforms = new Platforms(app, api, auth, companies);


app.MapGet("/ping", () => Results.Ok());
app.Use(async (context, next) =>
{
	var logger = app.Logger;

	logger.LogInformation("HTTP {Method} {Path} started", context.Request.Method, context.Request.Path);

	await next(); // передаем управление дальше по конвейеру

	logger.LogInformation("HTTP {Method} {Path} finished with status {StatusCode}",
		context.Request.Method, context.Request.Path, context.Response.StatusCode);
});


app.Run();
