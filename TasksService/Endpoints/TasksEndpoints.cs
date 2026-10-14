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
        Guid id, 
        HttpRequest http, 
        TasksDbContext db,
        ILogger<Program> logger
        )
    {
        //await db.TaskItems.;
        return Results.Ok(new AckResult(true));
    }

    private static async Task<IResult> CompleteAsync(
        Guid id, 
        TasksDbContext db,
        ILogger<Program> logger
        )
    {
        //await db.TaskItems.;
        return Results.Ok(new AckResult(true));
    }

    private static async Task<IResult> DeleteAsync(
        Guid id, 
        TasksDbContext db,
        ILogger<Program> logger
        )
    {
        //await db.TaskItems.;
        return Results.Ok(new AckResult(true));
    }

    // Получить все таски
    private static async Task<IResult> GetAllAsync(TasksDbContext db)
    {
        return Results.Ok(await db.TaskItems.Take(10).ToListAsync());
    }
}
