using Microsoft.EntityFrameworkCore;
using TaskItemService.Data;
using TaskService.Domain;
using TaskService.Models;

namespace TaskService.Endpoints;

public static class TasksEndpoints
{
    public static void MapTasksEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tasks");

        group.MapPost("/", CreateAsync);
        group.MapGet("/", GetAllAsync);
        group.MapPut("/{id:guid}/complete", CompleteAsync);
        group.MapDelete("/{id:guid}/", DeleteAsync);
    }

    private static async Task<IResult> CreateAsync(
        CreateTaskRequest request,
        TasksDbContext db,
        ILogger<Program> logger)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            IsCompleted = request.IsCompleted,
            Priority = request.Priority,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await db.TaskItems.AddAsync(task);
        await db.SaveChangesAsync();

        logger.LogInformation("Task {TaskId} created.", task.Id);

        return Results.Ok(new CreateTaskResponse(task.Id, task.Title, task.IsCompleted, task.Priority));
    }

    private static async Task<IResult> CompleteAsync(
        Guid id, 
        TasksDbContext db,
        IMessageBrocker messageBrocker,
        ILogger<Program> logger
        )
    {
        var existing = await db.TaskItems.FirstOrDefaultAsync(r => r.Id == id);

        if (existing is null)
            return Results.NotFound();
        
        if (existing.IsCompleted)
            return Results.Ok(true);

        existing.IsCompleted = true;
        existing.CompletedAt = DateTime.Now;
        await db.SaveChangesAsync();
        logger.LogInformation("Task {id} completed.", id);
        messageBrocker.SendTaskItemMessage(
            new TaskItemMessage(existing.Id.ToString(), existing.Title, existing.IsCompleted, existing.Priority.ToString())
        );
        logger.LogInformation("Task {id} is sent to queue.", id);

        return Results.Ok(true);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id, 
        TasksDbContext db,
        ILogger<Program> logger
        )
    {
        var existing = await db.TaskItems.FirstOrDefaultAsync(r => r.Id == id);

        if (existing is null)
            return Results.NotFound();
        
        db.Remove(existing);
        await db.SaveChangesAsync();
        logger.LogInformation("Task {id} deleted.", id);

        return Results.Ok(true);
    }

    // Получить все таски
    private static async Task<IResult> GetAllAsync(TasksDbContext db)
    {
        return Results.Ok(
            await 
                db.TaskItems.Take(100) // HACK: pagination
                .ToListAsync());
    }
}
