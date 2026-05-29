using Microsoft.EntityFrameworkCore;
using Order.API.Services;
using Order.Domain.Interfaces;
using Order.Infrastructure.Data;
using Order.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(connectionString) && connectionString.StartsWith("Host="))
{
    builder.Services.AddDbContext<OrderDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    var sqlitePath = connectionString ?? "Data Source=orders.db";
    builder.Services.AddDbContext<OrderDbContext>(options =>
        options.UseSqlite(sqlitePath));
}

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();
