using Microsoft.EntityFrameworkCore;
using TaskItemService.Data;

namespace TaskService.Data;

/// <summary>
/// Инициализация базы "paymentdb".
/// </summary>
public class DbInitializer
{
    private readonly TasksDbContext _db;
    private readonly ILogger<DbInitializer> _logger;

    public DbInitializer(TasksDbContext db, ILogger<DbInitializer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        // 1. Checks if DB is in-memory (e.g. during WebApplicationFactory unit tests)
        // In-memory providers don't support relational migrations.
        if (_db.Database.IsRelational())
        {
            // Creates the DB if it does not exist and applies all pending migrations
            await _db.Database.MigrateAsync();
            _logger.LogInformation("Database migrated successfully.");
        }
        else
        {
            await _db.Database.EnsureCreatedAsync();
            _logger.LogInformation("In-memory database ensured.");
        }
    }
}