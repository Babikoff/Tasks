using System.Security.Claims;
using System.Text.Json;
using Common;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OutBox;
using RabbitMQ.Client;
using TaskItemService.Data;
using TaskService.Data;
using TaskService.Endpoints;
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
    services.AddSingleton<IMessageBroker>(sp =>
    {
        var connection = sp.GetRequiredService<IConnection>();
        var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        var logger = sp.GetRequiredService<ILogger<RabbitMqService.RabbitMqService>>();
        return new RabbitMqService.RabbitMqService(connection, options.QueueName, logger);
    });
}


var builder = WebApplication.CreateBuilder(args);

// Настройки Postgres
var conn = builder.Configuration.GetConnectionString("Tasks");
conn ??= "Host=postgres;Port=5432;Database=tasksdb;Username=tasks;Password=password";
builder.Services.AddDbContext<TasksDbContext>(o => o.UseNpgsql(conn));

// Регистрация Transactional Outbox
builder.Services.AddTransactionalOutbox<TasksDbContext>();

// Services.
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskRequestValidator>();
builder.Services.AddScoped<DbInitializer>();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tasks API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Keycloak JWT Authentication & RBAC Authorization
var authAuthority = builder.Configuration["Authentication:Authority"] ?? "http://localhost:8081/realms/tasks-realm";
var authAudience = builder.Configuration["Authentication:Audience"] ?? "tasksservice";
var requireHttpsMetadata = builder.Configuration.GetValue<bool>("Authentication:RequireHttpsMetadata", false);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authAuthority;
        options.Audience = authAudience;
        options.RequireHttpsMetadata = requireHttpsMetadata;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                "http://keycloak:8080/realms/tasks-realm",
                "http://localhost:8081/realms/tasks-realm",
                "http://localhost/realms/tasks-realm",
                "https://localhost/realms/tasks-realm"
            },
            ValidateAudience = false,
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    var realmAccessClaim = context.Principal.FindFirst("realm_access")?.Value;
                    if (!string.IsNullOrEmpty(realmAccessClaim))
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(realmAccessClaim);
                            if (doc.RootElement.TryGetProperty("roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var role in rolesElement.EnumerateArray())
                                {
                                    var roleName = role.GetString();
                                    if (!string.IsNullOrEmpty(roleName) && !identity.HasClaim(ClaimTypes.Role, roleName))
                                    {
                                        identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                                    }
                                }
                            }
                        }
                        catch { }
                    }

                    var rolesClaims = context.Principal.FindAll("roles").ToList();
                    foreach (var roleClaim in rolesClaims)
                    {
                        if (!identity.HasClaim(ClaimTypes.Role, roleClaim.Value))
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                        }
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Worker - может вызывать GET /tasks и PUT /tasks/{id}/complete
    // Operator - может вызывать GET /tasks и POST /tasks
    // Manager - может вызывать все функции
    options.AddPolicy("CanViewTasks", policy => policy.RequireRole("Worker", "Operator", "Manager"));
    options.AddPolicy("CanCreateTasks", policy => policy.RequireRole("Operator", "Manager"));
    options.AddPolicy("CanCompleteTasks", policy => policy.RequireRole("Worker", "Manager"));
    options.AddPolicy("CanDeleteTasks", policy => policy.RequireRole("Manager"));
});

// Загрузка конфигурации сервиса очереди обмена данными
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));

// Регистрация сервиса очереди обмена данными
RegisterQueueService(builder.Services);

var app = builder.Build();

// --- Swagger / API Explorer Middleware ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

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


