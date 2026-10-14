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
        var created = await _db.Database.EnsureCreatedAsync();
        _logger.LogInformation("Payment database ensured (created: {Created}).", created);
    }
}