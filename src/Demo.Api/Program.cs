using Demo.Api.Middleware;
using Demo.Application.Modules;
using Demo.Domain.Modules.Inventory;
using Demo.Infrastructure;
using Demo.Infrastructure.Persistence;
using Hangfire;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<DemoDbContext>(options =>
    options.UseSqlite(connectionString));

// Repositories and modules — Scoped to align with the DbContext lifetime
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IInventoryModule, InventoryModule>();

// Hangfire
builder.Services.AddInfrastructureHangfire();

var app = builder.Build();

// Global error handling — must be first in the pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseHangfireDashboard();
app.MapControllers();

// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DemoDbContext>();
    await DatabaseSeeder.SeedAsync(db);
}

app.Run();
