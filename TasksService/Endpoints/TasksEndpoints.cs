using FluentValidation;
using MessageBrokers.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using TaskItemService.Data;
using TaskService.Common;
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
        group.MapGet("/{id:guid}/", GetAsync).WithName("GetTaskById");
        group.MapPut("/{id:guid}/complete", CompleteAsync);
        group.MapDelete("/{id:guid}/", DeleteAsync);
    }

    private static async Task<IResult> CreateAsync(
        CreateTaskRequest request,
        TasksDbContext db,
        ILogger<Program> logger,
        IValidator<CreateTaskRequest> validator,
        LinkGenerator linkGenerator,
        HttpContext httpContext
        )
    {
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid) 
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

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

        var path = linkGenerator.GetPathByName(httpContext, "GetTaskById", new { id = task.Id });

        return Results.Created(path, new CreateTaskResponse(task.Id, task.Title, task.IsCompleted, task.Priority, path ?? string.Empty));

        //return Results.CreatedAtRoute(
        //    routeName: "GetTaskById",
        //    routeValues: new { id = task.Id },
        //    value: new CreateTaskResponse(task.Id, task.Title, task.IsCompleted, task.Priority, path ?? string.Empty)
        //);
    }

    private static async Task<IResult> CompleteAsync(
        Guid id, 
        TasksDbContext db,
        IMessageBrokerWithOutbox messageBrocker,
        ILogger<Program> logger,
        CancellationToken ct
        )
    {
        var existing = await db.TaskItems.FirstOrDefaultAsync(r => r.Id == id);

        if (existing is null)
            return Results.NotFound();

        if (existing.IsCompleted)
            return Results.Ok(true);

        existing.IsCompleted = true;
        existing.CompletedAt = DateTime.Now;

        // Stage message in outbox within the same DbContext unit of work
        await messageBrocker.SendMessage(
            new TaskItemMessage(existing.Id, existing.Id.ToString(), existing.Title, existing.IsCompleted, existing.Priority.ToString()), ct
        );

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogInformation("Conflict on update of the Task {id}.", id);
            return Results.Conflict();
        }

        logger.LogInformation("Task {id} completed and staged to Outbox.", id);

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


    // Получить таск
    private static async Task<IResult> GetAsync(Guid id, TasksDbContext db)
    {
        var existing = await db.TaskItems.FirstOrDefaultAsync(r => r.Id == id);

        if (existing is null)
            return Results.NotFound();

        return Results.Ok(existing);
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
