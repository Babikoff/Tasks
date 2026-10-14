using TaskService.Common;

namespace TaskService.Models;

public record CreateTaskRequest(
    string Title, // не null, не пустой, максимум 200
    bool IsCompleted,
    Priority Priority
    );

public record CreateTaskResponse(
    Guid Id,
    string Title, // не null, не пустой, максимум 200
    bool IsCompleted,
    Priority Priority
    );


public record TaskItemMessage(
    string taskId,
    string title, // не null, не пустой, максимум 200
    bool isCompleted,
    string priority
    );

