using Microsoft.EntityFrameworkCore;
using TaskItemService.Data;
using TaskService.Data;
using TaskService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Настройки Postgres
//TODO: проверить нужна ли захардкоженная строка соединения 
var conn = builder.Configuration.GetConnectionString("Tasks")
    ?? "Host=postgres;Port=5432;Database=tasksdb;Username=tasks;Password=password";
builder.Services.AddDbContext<TasksDbContext>(o => o.UseNpgsql(conn));

// Services.
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddScoped<IMessageBrocker, RabbitMqSerivce>();

var app = builder.Build();

// Health check (для установки docker-compose итд).
app.MapGet("/health", () => Results.Ok("ok"));

// REST 
app.MapTasksEndpoints();

// Ensure the paymentdb database + schema on startup (prototype).
using (var scope = app.Services.CreateScope())
{
    var init = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    try { await init.InitializeAsync(); }
    catch (Exception ex)
    {
        var log = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        log.LogError(ex, "Database init failed.");
    }
}

app.Run();
