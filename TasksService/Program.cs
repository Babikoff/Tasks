using Common;
using Common.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OutBox;
using RabbitMQ.Client;
using RabbitMqService;
using TaskItemService.Data;
using TaskService.Data;
using TaskService.Endpoints;
using TaskService.Models;
using TaskService.Validation;


static void RegisterQueueService(IServiceCollection services)
{
    // Регистрация RabbitMQ connection as singleton
    services.AddSingleton<IConnection>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        var factory = new ConnectionFactory
        {
            HostName = options.HostName,
            Port = options.Port,
            UserName = options.UserName,
            Password = options.Password
        };
        return factory.CreateConnectionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    });

    // Регистрация RabbitMqService напрямую для отправки из фонового Outbox процессора
    services.AddSingleton<RabbitMqService.RabbitMqService>(sp =>
    {
        var connection = sp.GetRequiredService<IConnection>();
        var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<RabbitMqService.RabbitMqService>>();
        return new RabbitMqService.RabbitMqService(connection, options.QueueName, logger);
    });
}


var builder = WebApplication.CreateBuilder(args);

// Настройки Postgres
//TODO: проверить нужна ли захардкоженная строка соединения 
var conn = builder.Configuration.GetConnectionString("Tasks");
conn ??= "Host=postgres;Port=5432;Database=tasksdb;Username=tasks;Password=password";
builder.Services.AddDbContext<TasksDbContext>(o => o.UseNpgsql(conn));

// Регистрация Transactional Outbox
builder.Services.AddTransactionalOutbox<TasksDbContext>();

// Services.
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Загрузка конфигурации сервиса очереди обмена данными
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));

// Регистрация сервиса очереди обмена данными
RegisterQueueService(builder.Services);


var app = builder.Build();

// --- Added Swagger / API Explorer Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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

public partial class Program { }


