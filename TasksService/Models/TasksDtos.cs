using Common.Contracts;
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
    Priority Priority,
    string Url
    );


public record TaskItemMessage(
    Guid Id,
    string TaskId,
    string Title, // не null, не пустой, максимум 200
    bool IsCompleted,
    string Priority) : IMessage;

